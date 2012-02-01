using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Roger.Messages;
using Roger.Utilities;

namespace Roger.Internal.Impl
{
    internal class DefaultConsumingProcess : IConsumingProcess, IReceive<ConnectionEstablished>, IReceive<ConnectionUnexpectedShutdown>, IReceive<ConnectionGracefulShutdown>
    {
        private QueueingBasicConsumer queueConsumer;
        private readonly IConsumerContainer consumerContainer;
        private readonly ILog log = LogManager.GetCurrentClassLogger();
        private readonly IExchangeResolver exchangeResolver;
        private readonly IRoutingKeyResolver bindingKeyResolver;
        private readonly ITypeResolver typeResolver;
        private readonly IMessageSerializer serializer;
        private readonly IIdGenerator idGenerator;
        private readonly ISupportedMessageTypesResolver supportedMessageTypesResolver;
        private readonly IMessageFilter messageFilters;
        private readonly ConcurrentDictionary<WeakReference, object> instanceConsumers = new ConcurrentDictionary<WeakReference, object>();
        private readonly IQueueFactory queueFactory;
        private readonly RogerOptions options;
        private readonly IAggregator aggregator;
        private int disposed;
        private Task consumingTask;
        private readonly IConsumerInvoker consumerInvoker;

        [ThreadStatic]
        private static CurrentMessageInformation _currentMessage;

        public DefaultConsumingProcess(IIdGenerator idGenerator,
                                       IExchangeResolver exchangeResolver,
                                       IMessageSerializer serializer,
                                       ITypeResolver typeResolver,
                                       IConsumerContainer consumerContainer,
                                       IMessageFilter messageFilters,
                                       IQueueFactory queueFactory,
                                       IConsumerInvoker consumerInvoker,
                                       RogerOptions options,
                                       IAggregator aggregator)
        {
            this.consumerContainer = consumerContainer;
            this.queueFactory = queueFactory;
            this.consumerInvoker = consumerInvoker;
            this.options = options;
            this.aggregator = aggregator;
            this.exchangeResolver = exchangeResolver;
            bindingKeyResolver = new DefaultRoutingKeyResolver();
            this.typeResolver = typeResolver;
            this.serializer = serializer;
            this.idGenerator = idGenerator;
            supportedMessageTypesResolver = new DefaultSupportedMessageTypesResolver();
            this.messageFilters = messageFilters;
            aggregator.Subscribe(this);
        }

        public CurrentMessageInformation CurrentMessage
        {
            get { return _currentMessage; }
        }

        public RogerEndpoint Endpoint { get; private set; }

        public IModelWithConnection Model { get; private set; }

        void IReceive<ConnectionEstablished>.Receive(ConnectionEstablished message)
        {
            StartOrResumeConsumingFromConnection(message.Connection);
        }

        private void StartOrResumeConsumingFromConnection(IReliableConnection connection)
        {
            if (consumingTask != null)
            {
                log.Info("Consuming already running, letting current consuming loop complete and will restart consuming when completed");

                consumingTask = consumingTask.ContinueWith(task => StartConsuming(connection), TaskContinuationOptions.AttachedToParent);
                return;
            }

            StartConsuming(connection);
        }

        private void StartConsuming(IReliableConnection connection)
        {
            Model = connection.CreateModel();
            Model.ModelShutdown += ModelOnModelShutdown;

            if(options.PrefetchCount.HasValue)
            {
                log.InfoFormat("Setting QoS with prefetch count {0} on consuming channel", options.PrefetchCount);
                Model.BasicQos(0, options.PrefetchCount.Value, false);
            }

            Endpoint = new RogerEndpoint(Endpoint.IsEmpty ? queueFactory.Create(Model) : queueFactory.Create(Endpoint.Queue, Model));

            CreateBindings(new HashSet<Type>(consumerContainer.GetAllConsumerTypes().SelectMany(supportedMessageTypesResolver.Resolve)));
            log.DebugFormat("Declared and eventually bound endpoint {0}", Endpoint);

            aggregator.Notify(new ConsumingEnabled(Endpoint, connection));

            CreateConsumer();

            consumingTask = Task.Factory.StartNew(ConsumeSynchronously, TaskCreationOptions.LongRunning);
        }

        void IReceive<ConnectionUnexpectedShutdown>.Receive(ConnectionUnexpectedShutdown message)
        {
            // ignore model shutdown if caused by connection shutdown
            log.Debug("Ignoring model shutdown because connection was shut down unexpectedly");
            Model.ModelShutdown -= ModelOnModelShutdown;
        }

        void IReceive<ConnectionGracefulShutdown>.Receive(ConnectionGracefulShutdown message)
        {
            // ignore model shutdown if caused by connection shutdown
            log.Debug("Ignoring model shutdown because connection was shut down gracefully");
            Model.ModelShutdown -= ModelOnModelShutdown;
        }

        private void ModelOnModelShutdown(IModel model, ShutdownEventArgs reason)
        {
            if (disposed == 1)
                return;

            log.ErrorFormat("Model was shut down unexpectedly: {0}, {1}", model, reason);
            StartOrResumeConsumingFromConnection(Model.Connection);
        }

        private void CreateConsumer()
        {
            queueConsumer = new QueueingBasicConsumer(Model);

            try
            {
                Model.BasicConsume(Endpoint.Queue, false, "", options.NoLocal, false, null, queueConsumer);
            }
            catch (OperationInterruptedException e)
            {
                log.Error("Operation interrupted while invoking BasicConsume method", e);
            }
            catch (Exception e)
            {
                log.Error("Exception while invoking BasicConsume method", e);
            }
        }

        private void CreateBindings(ISet<Type> messageTypes)
        {
            // Here we allow eventual duplicate bindings if this method is called multiple times which result
            // in queues being bound to the same exchange with the same arguments
            // http://www.rabbitmq.com/amqp-0-9-1-reference.html#queue.bind

            if(!messageTypes.Any())
            {
                log.Debug("No binding to perform");
                return;
            }

            var allExchanges = new HashSet<string>();

            log.Debug("Performing standard bindings");

            foreach (var messageType in messageTypes)
            {
                var exchange = exchangeResolver.Resolve(messageType);
                allExchanges.Add(exchange);

                if (exchangeResolver.IsReply(messageType))
                    continue;

                var bindingKey = bindingKeyResolver.Resolve(messageType);

                log.DebugFormat("Binding queue {0} to exchange {1} with binding key {2}", Endpoint, exchange, bindingKey);

                Model.QueueBind(Endpoint.Queue, exchange, bindingKey);
            }

            log.Debug("Performing private bindings");

            foreach (var exchange in allExchanges)
            {
                log.DebugFormat("Binding queue {0} to exchange {1} with queue name as binding key", Endpoint, exchange);

                Model.QueueBind(Endpoint.Queue, exchange, Endpoint.Queue);
            }
        }

        private void ConsumeSynchronously()
        {
            log.Info("Beginning consuming loop");

            var messages = messageFilters.Filter(BlockingDequeue(queueConsumer.Queue), queueConsumer.Model);

            foreach (var message in messages.Where(SetCurrentMessageAndInvokeConsumers))
                AckMessage(message);

            log.Info("Exiting consuming loop");
        }

        private IEnumerable<CurrentMessageInformation> BlockingDequeue(IEnumerable queue)
        {
            return queue.Cast<BasicDeliverEventArgs>().Select(CreateMessage);
        }

        private CurrentMessageInformation CreateMessage(BasicDeliverEventArgs args)
        {
            var properties = args.BasicProperties;
            var messageType = typeResolver.Resolve(properties.Type);

            return new CurrentMessageInformation
            {
                MessageId = new RogerGuid(properties.MessageId),
                MessageType = messageType,
                Endpoint = new RogerEndpoint(properties.ReplyTo),
                CorrelationId = string.IsNullOrWhiteSpace(properties.CorrelationId) ? RogerGuid.Empty : new RogerGuid(properties.CorrelationId),
                DeliveryTag = args.DeliveryTag,
                Exchange = args.Exchange,
                Body = serializer.Deserialize(messageType, args.Body),
                Headers = (Hashtable)properties.Headers
            };
        }

        private bool SetCurrentMessageAndInvokeConsumers(CurrentMessageInformation message)
        {
            _currentMessage = message;

            IEnumerable<IConsumer> consumers;

            using(ResolveAndRelease(message.MessageType, out consumers))
                return consumerInvoker.Invoke(consumers, _currentMessage);
        }

        private IDisposable ResolveAndRelease(Type messageType, out IEnumerable<IConsumer> consumers)
        {
            var resolved = ResolveConsumers(messageType);
            consumers = resolved.All;

            return new DisposableAction(() => consumerContainer.Release(resolved.StandardConsumers));
        }

        private Consumers ResolveConsumers(Type messageType)
        {
            var localConsumers = InstanceConsumers(messageType).ToArray();
            var standardConsumers = consumerContainer.Resolve(messageType.HierarchyRoot()).Distinct().ToArray();

            log.TraceFormat("Found {0} standard consumers and {1} instance consumers for message {2}",
                            standardConsumers.Length,
                            localConsumers.Length,
                            messageType);

            return new Consumers(localConsumers, standardConsumers);
        }

        private IEnumerable<IConsumer> InstanceConsumers(Type messageType)
        {
            return instanceConsumers.Keys
                .Where(r => r.IsAlive)
                .Select(r => r.Target)
                .Cast<IConsumer>()
                .Where(c => GetSupportedMessageTypes(c).Contains(messageType));
        }

        private void AckMessage(CurrentMessageInformation message)
        {
            try
            {
                queueConsumer.Model.BasicAck(message.DeliveryTag, false);
            }
            catch (AlreadyClosedException e)
            {
                log.Trace("Could not ack consumed message because model was already closed", e);
            }
            catch (Exception e)
            {
                log.Warn("Could not ack consumed message for unknown cause", e);
            }
        }

        public IDisposable AddInstanceSubscription(IConsumer consumer)
        {
            var consumerReference = new WeakReference(consumer);
            instanceConsumers.TryAdd(consumerReference, null);

            log.InfoFormat("Added instance subscription for consumer of type {0}, hash {1}", consumer.GetType(), consumerReference.GetHashCode());

            CreateBindings(GetSupportedMessageTypes(consumer));

            // TODO: queue bindings are not removed, no problem unless we start adding too many instance subscriptions
            return RemoveInstanceConsumer(consumerReference);
        }

        public void Consume(object message)
        {
            SetCurrentMessageAndInvokeConsumers(new CurrentMessageInformation
            {
                MessageId = idGenerator.Next(),
                Body = message,
                MessageType = message.GetType()
            });
        }

        private IDisposable RemoveInstanceConsumer(WeakReference consumerReference)
        {
            return new DisposableAction(() =>
            {
                object @null;
                if(instanceConsumers.TryRemove(consumerReference, out @null))
                    log.InfoFormat("Removed instance subscription with hash {0}", consumerReference.GetHashCode());
            });
        }

        private ISet<Type> GetSupportedMessageTypes(IConsumer consumer)
        {
            return supportedMessageTypesResolver.Resolve(consumer.GetType());
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) == 1)
                return;

            DisposeModel();
            WaitForConsumingTask();
        }

        private void WaitForConsumingTask()
        {
            if (consumingTask == null)
                return;

            try
            {
                consumingTask.Wait(100);
            }
            catch (AggregateException e)
            {
                log.Error("Exception while waiting on consuming task", e.Flatten());
            }
        }

        private void DisposeModel()
        {
            if (Model == null) 
                return;

            try
            {
                // todo: we could try a second attempt on a standalone channel
                Model.QueueDelete(Endpoint);
                Model.Dispose();
            }
            catch (OperationInterruptedException e)
            {
                log.Error("Operation interrupted while deleting queue or disposing model", e);
            }
            catch (Exception e)
            {
                log.Error("Exception while deleting queue and disposing model", e);
            }
        }

        private class Consumers
        {
            public IConsumer[] LocalConsumers { get; private set; }
            public IConsumer[] StandardConsumers { get; private set; }

            public IEnumerable<IConsumer> All
            {
                get { return StandardConsumers.Concat(LocalConsumers); }
            }

            public Consumers(IConsumer[] localConsumers, IConsumer[] standardConsumers)
            {
                LocalConsumers = localConsumers;
                StandardConsumers = standardConsumers;
            }
        }
    }
}