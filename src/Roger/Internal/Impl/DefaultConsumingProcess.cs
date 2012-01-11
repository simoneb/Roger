using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Roger.Utilities;

namespace Roger.Internal.Impl
{
    internal class DefaultConsumingProcess : IConsumingProcess
    {
        private readonly IReliableConnection connection;
        private IModel receivingModel;
        private QueueingBasicConsumer queueConsumer;
        private readonly IConsumerContainer consumerContainer;
        private readonly IRogerLog log;
        private readonly IExchangeResolver exchangeResolver;
        private readonly IBindingKeyResolver bindingKeyResolver;
        private readonly ITypeResolver typeResolver;
        private readonly IMessageSerializer serializer;
        private readonly IReflection reflection;
        private readonly IIdGenerator idGenerator;
        private readonly ISupportedMessageTypesResolver supportedMessageTypesResolver;
        private readonly IEnumerable<IMessageFilter> messageFilters;
        private readonly ConcurrentDictionary<WeakReference, object> instanceConsumers = new ConcurrentDictionary<WeakReference, object>();
        private readonly IQueueFactory queueFactory;
        private readonly bool noLocal;

        [ThreadStatic]
        private static CurrentMessageInformation _currentMessage;

        private int disposed;
        private Task consumingTask;

        public DefaultConsumingProcess(IReliableConnection connection,
                                       IIdGenerator idGenerator,
                                       IExchangeResolver exchangeResolver,
                                       IMessageSerializer serializer,
                                       ITypeResolver typeResolver,
                                       IConsumerContainer consumerContainer,
                                       IReflection reflection,
                                       IEnumerable<IMessageFilter> messageFilters,
                                       IRogerLog log,
                                       IQueueFactory queueFactory,
                                       bool noLocal)
        {
            this.connection = connection;
            this.consumerContainer = consumerContainer;
            this.log = log;
            this.queueFactory = queueFactory;
            this.noLocal = noLocal;
            this.exchangeResolver = exchangeResolver;
            bindingKeyResolver = Default.BindingKeyResolver;
            this.typeResolver = typeResolver;
            this.serializer = serializer;
            this.reflection = reflection;
            this.idGenerator = idGenerator;
            supportedMessageTypesResolver = Default.SupportedMessageTypesResolver;
            this.messageFilters = messageFilters;

            connection.ConnectionEstabilished += ConnectionEstabilished;
        }

        private void ConnectionEstabilished()
        {
            receivingModel = connection.CreateModel();

            if(Endpoint.IsEmpty)
            {
                Endpoint = new RogerEndpoint(queueFactory.Create(receivingModel));
                CreateBindings(new HashSet<Type>(consumerContainer.GetAllConsumerTypes().SelectMany(supportedMessageTypesResolver.Resolve)));
                log.DebugFormat("Created and bound queue {0}", Endpoint);
            }

            CreateConsumer();
            ConsumeAsynchronously();
        }

        private void CreateConsumer()
        {
            queueConsumer = new QueueingBasicConsumer(receivingModel);

            try
            {
                receivingModel.BasicConsume(Endpoint.Queue, false, "", noLocal, false, null, queueConsumer);
            }
            catch (OperationInterruptedException e)
            {
                log.ErrorFormat("Operation interrupted while invoking BasicConsume method\r\n{0}", e);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Exception while invoking BasicConsume method\r\n{0}", e);
            }
        }

        public RogerEndpoint Endpoint { get; private set; }

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

                if (messageType.IsReply())
                    continue;

                var bindingKey = bindingKeyResolver.Resolve(messageType);

                log.DebugFormat("Binding queue {0} to exchange {1} with binding key {2}", Endpoint, exchange, bindingKey);

                receivingModel.QueueBind(Endpoint.Queue, exchange, bindingKey);
            }

            log.Debug("Performing private bindings");

            foreach (var exchange in allExchanges)
            {
                log.DebugFormat("Binding queue {0} to exchange {1} with queue name as binding key", Endpoint, exchange);

                receivingModel.QueueBind(Endpoint.Queue, exchange, Endpoint.Queue);
            }
        }

        private void ConsumeAsynchronously()
        {
            consumingTask = Task.Factory.StartNew(ConsumeSynchronously, TaskCreationOptions.LongRunning);
        }

        private void ConsumeSynchronously()
        {
            var messages = messageFilters.Aggregate(BlockingDequeue(queueConsumer.Queue), (current, filter) => filter.Filter(current, queueConsumer.Model));

            foreach (var message in messages)
            {
                SetCurrentMessageAndInvokeConsumers(message);

                try
                {
                    queueConsumer.Model.BasicAck(message.DeliveryTag, false);
                }
                catch (AlreadyClosedException e)
                {
                    log.ErrorFormat("Could not ack consumed message because model was already closed\r\n{0}", e);
                }
                catch (Exception e)
                {
                    log.ErrorFormat("Could not ack consumed message\r\n{0}", e);
                }
            }
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

        private void SetCurrentMessageAndInvokeConsumers(CurrentMessageInformation message)
        {
            _currentMessage = message;

            var consumers = ResolveConsumers(_currentMessage.MessageType);

            var localInstanceConsumers = consumers.Item1.ToArray();
            var defaultConsumers = consumers.Item2.ToArray();

            log.DebugFormat("Found {0} standard consumers and {1} instance consumers for message {2}",
                            defaultConsumers.Length,
                            localInstanceConsumers.Length,
                            _currentMessage.MessageType);

            var allConsumers = localInstanceConsumers.Concat(defaultConsumers);

            foreach (var c in allConsumers)
            {
                log.DebugFormat("Invoking Consume method on consumer {0} for message {1}",
                                c.GetType(),
                                _currentMessage.MessageType);

                reflection.InvokeConsume(c, _currentMessage.Body);
            }

            consumerContainer.Release(defaultConsumers);
        }

        private Tuple<IEnumerable<IConsumer>, IEnumerable<IConsumer>> ResolveConsumers(Type messageType)
        {
            return Tuple.Create(InstanceConsumers(messageType), reflection.Hierarchy(messageType).ConsumerOf().SelectMany(consumerContainer.Resolve).Distinct());
        }

        private IEnumerable<IConsumer> InstanceConsumers(Type messageType)
        {
            return instanceConsumers.Keys
                .Where(r => r.IsAlive)
                .Select(r => r.Target)
                .Cast<IConsumer>()
                .Where(c => GetSupportedMessageTypes(c).Contains(messageType));
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

        public CurrentMessageInformation CurrentMessage
        {
            get { return _currentMessage; }
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

            if(receivingModel != null)
            {
                try
                {
                    receivingModel.QueueDelete(Endpoint);
                    receivingModel.Dispose();
                }
                catch (OperationInterruptedException e)
                {
                    log.ErrorFormat("Operation interrupted while deleting queue or disposing model\r\n{0}", e);
                }
                catch(Exception e)
                {
                    log.ErrorFormat("Exception while deleting queue and disposing model\r\n{0}", e);
                }
            }

            if (consumingTask != null)
                consumingTask.Wait(100);
        }
    }
}