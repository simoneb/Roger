using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Rabbus.Errors;
using Rabbus.Logging;
using Rabbus.Reflection;
using Rabbus.Resolvers;
using Rabbus.Serialization;
using Rabbus.Utilities;

namespace Rabbus
{
    public class DefaultRabbitBus : IRabbitBus
    {
        private delegate Tuple<IEnumerable<IConsumer>, IEnumerable<IConsumer>> ConsumerResolver(Type messageType);

        private readonly IConnection connection;
        private readonly IRoutingKeyResolver routingKeyResolver;
        private readonly ITypeResolver typeResolver;
        private readonly IMessageSerializer serializer;
        private readonly IReflection reflection;
        private readonly IConsumerResolver consumerResolver;
        private readonly ISupportedMessageTypesResolver supportedMessageTypesResolver;
        private readonly IExchangeResolver exchangeResolver;
        private readonly IRabbusLog log;

        public CurrentMessageInformation CurrentMessage { get { return currentMessage; } }
        public RabbusEndpoint LocalEndpoint { get; private set; }

        [ThreadStatic]
        private static CurrentMessageInformation currentMessage;

        private readonly ConcurrentDictionary<WeakReference, object> instanceConsumers = new ConcurrentDictionary<WeakReference, object>();
        private IModel localModel;

        public DefaultRabbitBus(IConnectionFactory connectionFactory,
                                IConsumerResolver consumerResolver = null,
                                ITypeResolver typeResolver = null,
                                ISupportedMessageTypesResolver supportedMessageTypesResolver = null,
                                IExchangeResolver exchangeResolver = null,
                                IRoutingKeyResolver routingKeyResolver = null,
                                IMessageSerializer serializer = null,
                                IReflection reflection = null,
                                IRabbusLog log = null)
        {
            this.consumerResolver = consumerResolver ?? new OneWayBusConsumerResolver();
            this.typeResolver = typeResolver ?? new DefaultTypeResolver();
            this.supportedMessageTypesResolver = supportedMessageTypesResolver ?? new DefaultSupportedMessageTypesResolver();
            this.exchangeResolver = exchangeResolver ?? new DefaultExchangeResolver();
            this.reflection = reflection ?? new DefaultReflection();
            this.routingKeyResolver = routingKeyResolver ?? new DefaultRoutingKeyResolver();
            this.serializer = serializer ?? new ProtoBufNetSerializer();
            this.log = log ?? new NullLog();

            connection = connectionFactory.CreateConnection();
        }

        public void Initialize()
        {
            log.Debug("Initializing bus");

            localModel = NewModel;
            LocalEndpoint = new RabbusEndpoint(localModel.QueueDeclare("", false, true, false, null));

            var consumer = Subscribe(consumerResolver.GetAllConsumersTypes());

            ConsumeAsynchronously(ResolveConsumers, consumer);

            log.Debug("Bus initialization completed");
        }

        private IModel NewModel { get { return connection.CreateModel(); } }

        private QueueingBasicConsumer Subscribe(IEnumerable<Type> consumerTypes)
        {
            var allMessageTypes = consumerTypes.SelectMany(GetSupportedMessageTypes).Distinct();

            AddBindings(allMessageTypes);

            var consumer = new QueueingBasicConsumer(localModel);
            localModel.BasicConsume(LocalEndpoint.Queue, false, consumer);
            return consumer;
        }

        private void AddBindings(IEnumerable<Type> messageTypes)
        {
            // Here we allow eventual duplicate bindings if this method is called multiple times which result
            // in queues being bound to the same exchange with the same arguments
            // http://www.rabbitmq.com/amqp-0-9-1-reference.html#queue.bind

            var allExchanges = new HashSet<string>();

            log.Debug("Performing pub/sub bindings");

            foreach (var messageType in messageTypes.ExceptReplies())
            {
                var exchange = exchangeResolver.Resolve(messageType);
                var routingKey = routingKeyResolver.Resolve(messageType);

                log.DebugFormat("Binding queue {0} to exchange {1} with routing key {2}", LocalEndpoint, exchange, routingKey);

                localModel.QueueBind(LocalEndpoint.Queue, exchange, routingKey);

                allExchanges.Add(exchange);
            }

            log.Debug("Performing private messages bindings");

            foreach (var exchange in allExchanges)
            {
                log.DebugFormat("Binding queue {0} to exchange {1} with routing key {2}", LocalEndpoint, exchange, LocalEndpoint);

                localModel.QueueBind(LocalEndpoint.Queue, exchange, LocalEndpoint.Queue);
            }
        }

        private void ConsumeAsynchronously(ConsumerResolver resolveConsumers, QueueingBasicConsumer queueConsumer)
        {
            Task.Factory.StartNew(() => ConsumeSynchronously(resolveConsumers, queueConsumer), TaskCreationOptions.LongRunning);
        }

        private void ConsumeSynchronously(ConsumerResolver resolveConsumers, QueueingBasicConsumer consumer)
        {
            foreach (var message in BlockingDequeue(consumer.Queue))
            {
                SetCurrentMessageAndInvokeConsumers(resolveConsumers, message);
                consumer.Model.BasicAck(message.DeliveryTag, false);
            }
        }

        private IEnumerable<CurrentMessageInformation> BlockingDequeue(IEnumerable queue)
        {
            return from args in queue.OfType<BasicDeliverEventArgs>()
                   let messageType = typeResolver.ResolveType(args.BasicProperties.Type)
                   select new CurrentMessageInformation
                          {
                              MessageType = messageType,
                              Endpoint = new RabbusEndpoint(args.BasicProperties.ReplyTo),
                              CorrelationId = string.IsNullOrWhiteSpace(args.BasicProperties.CorrelationId)
                                                  ? Guid.Empty
                                                  : new Guid(args.BasicProperties.CorrelationId),
                              DeliveryTag = args.DeliveryTag,
                              Exchange = args.Exchange,
                              Body = serializer.Deserialize(messageType, args.Body)
                          };
        }

        private void SetCurrentMessageAndInvokeConsumers(ConsumerResolver resolveConsumers, CurrentMessageInformation message)
        {
            currentMessage = message;

            var consumers = resolveConsumers(currentMessage.MessageType);

            var localInstanceConsumers = consumers.Item1.ToArray();
            var defaultConsumers = consumers.Item2.ToArray();

            log.DebugFormat("Found {0} instance consumers and {1} default consumers for message {2}",
                            localInstanceConsumers.Length,
                            defaultConsumers.Length,
                            currentMessage.MessageType);

            var allConsumers = localInstanceConsumers.Concat(defaultConsumers);

            foreach (var c in allConsumers)
            {
                log.DebugFormat("Invoking Consume method on consumer {0} for message {1}",
                                c.GetType(),
                                currentMessage.MessageType);

                reflection.InvokeConsume(c, currentMessage.Body);
            }

            consumerResolver.Release(defaultConsumers);
        }

        public IDisposable AddInstanceSubscription(IConsumer consumer)
        {
            var consumerReference = new WeakReference(consumer);
            instanceConsumers.TryAdd(consumerReference, null);

            AddBindings(GetSupportedMessageTypes(consumer));

            // TODO: queue bindings are not removed, no problem unless we start adding too many instance subscriptions
            return RemoveInstanceConsumer(consumerReference);
        }

        private IDisposable RemoveInstanceConsumer(WeakReference consumerReference)
        {
            return new DisposableAction(() =>
            {
                object _;
                instanceConsumers.TryRemove(consumerReference, out _);
            });
        }

        private IEnumerable<Type> GetSupportedMessageTypes(IConsumer consumer)
        {
            return supportedMessageTypesResolver.Get(consumer.GetType());
        }

        private IEnumerable<Type> GetSupportedMessageTypes(Type consumerType)
        {
            return supportedMessageTypesResolver.Get(consumerType);
        }

        public void Publish(object message)
        {
            using (var model = NewModel)
            {
                var messageType = message.GetType();
                var properties = PopulateProperties(model, messageType);

                model.BasicPublish(exchangeResolver.Resolve(messageType),
                                   routingKeyResolver.Resolve(messageType),
                                   properties,
                                   serializer.Serialize(message));
            }
        }

        private IBasicProperties PopulateProperties(IModel model, Type messageType)
        {
            var properties = model.CreateBasicProperties();

            properties.Type = typeResolver.Unresolve(messageType);
            properties.ReplyTo = LocalEndpoint.Queue;

            return properties;
        }

        public void PublishMandatory(object message, Action<PublishFailureReason> publishFailure)
        {
            using (var model = NewModel)
            {
                var properties = PopulateProperties(model, message.GetType());

                PublishMandatoryInternal(message, properties, model, publishFailure);
                // disposing here, is this correct? what if BasicReturn needs to be invoked by a disposed model?
                // tests succeed, BTW
            }
        }

        private void PublishMandatoryInternal(object message,
                                              IBasicProperties properties,
                                              IModel model,
                                              Action<PublishFailureReason> publishFailure)
        {
            PublishMandatoryInternal(message, properties, model, publishFailure, routingKeyResolver.Resolve(message.GetType()));
        }

        private void PublishMandatoryInternal(object message,
                                              IBasicProperties properties,
                                              IModel model,
                                              Action<PublishFailureReason> publishFailure,
                                              string routingKey)
        {
            CallbackOnBasicReturn(model, publishFailure);

            model.BasicPublish(exchangeResolver.Resolve(message.GetType()),
                               routingKey,
                               true,
                               false,
                               properties,
                               serializer.Serialize(message));
        }

        public void Send(RabbusEndpoint endpoint, object message)
        {
            Send(endpoint, message, Nop);
        }

        public void Send(RabbusEndpoint endpoint, object message, Action<PublishFailureReason> publishFailure)
        {
            using (var model = NewModel)
            {
                var properties = PopulateProperties(model, message.GetType());

                PublishMandatoryInternal(message, properties, model, publishFailure, endpoint.Queue);
            }
        }

        private static void Nop<T>(T input)
        {}

        public void Request(object message)
        {
            Request(message, _ => {});
        }

        public void Request(object message, Action<PublishFailureReason> requestFailure)
        {
            using (var model = NewModel)
            {
                var properties = PopulateProperties(model, message.GetType());
                properties.CorrelationId = Guid.NewGuid().ToString();

                PublishMandatoryInternal(message, properties, model, requestFailure);

                log.DebugFormat("Issued request with message {0}", message.GetType());
            }
        }

        public void Reply(object message)
        {
            if (CurrentMessage == null || 
                CurrentMessage.Endpoint.IsEmpty() ||
                CurrentMessage.CorrelationId.IsEmpty())
            {
                log.Error("Reply method called out of the context of a message handling request");
                throw new InvalidOperationException(ErrorMessages.ReplyInvokedOutOfRequestContext);
            }

            using (var model = NewModel)
            {
                var properties = PopulateProperties(model, message.GetType());
                properties.CorrelationId = CurrentMessage.CorrelationId.ToString();

                model.BasicPublish(exchangeResolver.Resolve(CurrentMessage.MessageType),
                                   CurrentMessage.Endpoint,
                                   properties,
                                   serializer.Serialize(message));
            }
        }

        private Tuple<IEnumerable<IConsumer>, IEnumerable<IConsumer>> ResolveConsumers(Type messageType)
        {
            return Tuple.Create(instanceConsumers.Keys
                                    .Where(r => r.IsAlive)
                                    .Select(r => r.Target)
                                    .Cast<IConsumer>()
                                    .Where(c => GetSupportedMessageTypes(c).Any(m => m.Equals(messageType))),
                                consumerResolver.Resolve(messageType));
        }

        private void CallbackOnBasicReturn(IModel model, Action<PublishFailureReason> publishFailure)
        {
            model.BasicReturn += (_, args) =>
            {
                try
                {
                    log.DebugFormat("Model issued a basic return for message {{we can do better here}} with reply {0} - {1}", args.ReplyCode, args.ReplyText);
                    publishFailure(new PublishFailureReason(args.ReplyCode, args.ReplyText));
                }
                finally
                {
                    model.Dispose();
                }
            };
        }

        public void Consume(object message)
        {
            SetCurrentMessageAndInvokeConsumers(ResolveConsumers, new CurrentMessageInformation
            {
                Body = message,
                MessageType = message.GetType()
            });
        }

        public void Dispose()
        {
            log.Debug("Disposing bus");

            if(connection != null && connection.IsOpen)
                connection.Dispose();
        }
    }
}