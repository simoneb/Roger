using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Rabbus.Connection;
using Rabbus.GuidGeneration;
using Rabbus.Logging;
using Rabbus.Reflection;
using Rabbus.Resolvers;
using Rabbus.Serialization;
using Rabbus.Utilities;

namespace Rabbus.Consuming
{
    internal class DefaultConsumingProcess : IConsumingProcess
    {
        private readonly IReliableConnection connection;
        private IModel receivingModel;
        private QueueingBasicConsumer queueConsumer;
        private readonly IConsumerResolver consumerResolver;
        private readonly IRabbusLog log;
        private readonly IExchangeResolver exchangeResolver;
        private readonly IRoutingKeyResolver routingKeyResolver;
        private readonly ITypeResolver typeResolver;
        private readonly IMessageSerializer serializer;
        private readonly IReflection reflection;
        private readonly IGuidGenerator guidGenerator;
        private readonly ISupportedMessageTypesResolver supportedMessageTypesResolver;
        private readonly ConcurrentDictionary<WeakReference, object> instanceConsumers = new ConcurrentDictionary<WeakReference, object>();

        [ThreadStatic]
        private static CurrentMessageInformation _currentMessage;

        public DefaultConsumingProcess(IReliableConnection connection,
                                       IGuidGenerator guidGenerator,
                                       IExchangeResolver exchangeResolver,
                                       IRoutingKeyResolver routingKeyResolver,
                                       IMessageSerializer serializer,
                                       ITypeResolver typeResolver,
                                       IConsumerResolver consumerResolver,
                                       IReflection reflection,
                                       ISupportedMessageTypesResolver supportedMessageTypesResolver,
                                       IRabbusLog log)
        {
            this.connection = connection;
            this.consumerResolver = consumerResolver;
            this.log = log;
            this.exchangeResolver = exchangeResolver;
            this.routingKeyResolver = routingKeyResolver;
            this.typeResolver = typeResolver;
            this.serializer = serializer;
            this.reflection = reflection;
            this.guidGenerator = guidGenerator;
            this.supportedMessageTypesResolver = supportedMessageTypesResolver;

            connection.ConnectionEstabilished += HandleConnectionEstabilished;
        }

        private void HandleConnectionEstabilished()
        {
            receivingModel = connection.CreateModel();

            Endpoint = new RabbusEndpoint(receivingModel.QueueDeclare("", false, true, false, null));

            CreateConsumer();
            ConsumeAsynchronously();
        }

        private void CreateConsumer()
        {
            CreateBindings(consumerResolver.GetAllSupportedMessageTypes());

            queueConsumer = new QueueingBasicConsumer(receivingModel);
            receivingModel.BasicConsume(Endpoint.Queue, false, queueConsumer);
        }

        public RabbusEndpoint Endpoint { get; private set; }

// ReSharper disable ParameterTypeCanBeEnumerable.Local
        private void CreateBindings(ISet<Type> messageTypes)
// ReSharper restore ParameterTypeCanBeEnumerable.Local
        {
            // Here we allow eventual duplicate bindings if this method is called multiple times which result
            // in queues being bound to the same exchange with the same arguments
            // http://www.rabbitmq.com/amqp-0-9-1-reference.html#queue.bind

            var allExchanges = new HashSet<string>();

            log.Debug("Performing pub/sub bindings");

            foreach (var messageType in messageTypes)
            {
                var exchange = exchangeResolver.Resolve(messageType);
                allExchanges.Add(exchange);

                if (messageType.IsReply())
                    continue;

                var routingKey = routingKeyResolver.Resolve(messageType);

                log.DebugFormat("Binding queue {0} to exchange {1} with routing key {2}", Endpoint, exchange, routingKey);

                receivingModel.QueueBind(Endpoint.Queue, exchange, routingKey);
            }

            log.Debug("Performing private messages bindings");

            foreach (var exchange in allExchanges)
            {
                log.DebugFormat("Binding queue {0} to exchange {1} with quete name as routing key", Endpoint, exchange);

                receivingModel.QueueBind(Endpoint.Queue, exchange, Endpoint.Queue);
            }
        }

        private void ConsumeAsynchronously()
        {
            Task.Factory.StartNew(ConsumeSynchronously, TaskCreationOptions.LongRunning);
        }

        private void ConsumeSynchronously()
        {
            foreach (var message in BlockingDequeue(queueConsumer.Queue))
            {
                SetCurrentMessageAndInvokeConsumers(message);
                queueConsumer.Model.BasicAck(message.DeliveryTag, false);
            }
        }

        private IEnumerable<CurrentMessageInformation> BlockingDequeue(IEnumerable queue)
        {
            return queue.OfType<BasicDeliverEventArgs>().Select(CreateMessage);
        }

        private CurrentMessageInformation CreateMessage(BasicDeliverEventArgs args)
        {
            var properties = args.BasicProperties;
            var messageType = typeResolver.ResolveType(properties.Type);

            return new CurrentMessageInformation
            {
                MessageId = new RabbusGuid(properties.MessageId),
                MessageType = messageType,
                Endpoint = new RabbusEndpoint(properties.ReplyTo),
                CorrelationId = string.IsNullOrWhiteSpace(properties.CorrelationId) ? RabbusGuid.Empty : new RabbusGuid(properties.CorrelationId),
                DeliveryTag = args.DeliveryTag,
                Exchange = args.Exchange,
                Body = serializer.Deserialize(messageType, args.Body)
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

            consumerResolver.Release(defaultConsumers);
        }

        private Tuple<IEnumerable<IConsumer>, IEnumerable<IConsumer>> ResolveConsumers(Type messageType)
        {
            return Tuple.Create(InstanceConsumers(messageType), consumerResolver.Resolve(messageType));
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

            CreateBindings(GetSupportedMessageTypes(consumer));

            // TODO: queue bindings are not removed, no problem unless we start adding too many instance subscriptions
            return RemoveInstanceConsumer(consumerReference);
        }

        public void Consume(object message)
        {
            SetCurrentMessageAndInvokeConsumers(new CurrentMessageInformation
            {
                MessageId = guidGenerator.Next(),
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
                object _;
                instanceConsumers.TryRemove(consumerReference, out _);
            });
        }

        private ISet<Type> GetSupportedMessageTypes(IConsumer consumer)
        {
            return supportedMessageTypesResolver.Resolve(consumer.GetType());
        }
    }
}