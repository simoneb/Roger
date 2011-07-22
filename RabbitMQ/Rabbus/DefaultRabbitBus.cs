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

        [ThreadStatic]
        private static CurrentMessageInformation currentMessage;

        private readonly ConcurrentDictionary<WeakReference, IModel> instanceConsumers = new ConcurrentDictionary<WeakReference, IModel>();
        private IModel mainModel;
        private const string DefaultExchange = "";

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

            var allMessageTypes = (from consumerType in consumerResolver.GetAllConsumersTypes()
                                   from messageType in supportedMessageTypesResolver.Get(consumerType)
                                   select messageType).Distinct();

            mainModel = CreateModel();
            var queue = mainModel.QueueDeclare("", false, true, true, null);

            var consumer = Subscribe(mainModel, allMessageTypes, queue);

            ConsumeAsynchronously(ResolveConsumers, consumer, Identity, Identity);

            log.Debug("Bus initialization completed");
        }

        private IModel CreateModel()
        {
            return connection.CreateModel();
        }

        private QueueingBasicConsumer Subscribe(IModel model, IEnumerable<Type> messageTypes, string queue)
        {
            foreach (var messageType in messageTypes)
                model.QueueBind(queue, exchangeResolver.Resolve(messageType), routingKeyResolver.Resolve(messageType));

            var consumer = new QueueingBasicConsumer(model);
            model.BasicConsume(queue, false, consumer);
            return consumer;
        }

        private Task ConsumeAsynchronously(Func<Type, Tuple<IEnumerable<IConsumer>, IEnumerable<IConsumer>>> resolveConsumers,
                                           QueueingBasicConsumer queueConsumer,
                                           Func<IEnumerable<BasicDeliverEventArgs>, IEnumerable<BasicDeliverEventArgs>> messageFilter, 
                                           Func<IEnumerable<IConsumer>, IEnumerable<IConsumer>> consumerFilter)
        {
            return Task.Factory.StartNew(() => ConsumeSynchronously(resolveConsumers, queueConsumer, messageFilter, consumerFilter));
        }

        private void ConsumeSynchronously(Func<Type, Tuple<IEnumerable<IConsumer>, IEnumerable<IConsumer>>> resolveConsumers,
                                          QueueingBasicConsumer consumer,
                                          Func<IEnumerable<BasicDeliverEventArgs>, IEnumerable<BasicDeliverEventArgs>> messageFilter, 
                                          Func<IEnumerable<IConsumer>, IEnumerable<IConsumer>> consumerFilter)
        {
            foreach (var message in DequeMessagesSynchronously(messageFilter, consumer.Queue))
            {
                InvokeConsumers(resolveConsumers, consumerFilter, message);
                consumer.Model.BasicAck(currentMessage.DeliveryTag, false);
            }
        }

        private IEnumerable<CurrentMessageInformation> DequeMessagesSynchronously(Func<IEnumerable<BasicDeliverEventArgs>, IEnumerable<BasicDeliverEventArgs>> messageFilter,
                                                                                  IEnumerable queue)
        {
            return from args in messageFilter(queue.OfType<BasicDeliverEventArgs>())
                   select new CurrentMessageInformation
                          {
                              MessageType = typeResolver.ResolveType(args.BasicProperties.Type),
                              ReplyTo = args.BasicProperties.ReplyTo,
                              CorrelationId = args.BasicProperties.CorrelationId,
                              DeliveryTag = args.DeliveryTag,
                              Exchange = args.Exchange,
                              Body = serializer.Deserialize(typeResolver.ResolveType(args.BasicProperties.Type), args.Body)
                          };
        }

        private void InvokeConsumers(Func<Type, Tuple<IEnumerable<IConsumer>, IEnumerable<IConsumer>>> resolveConsumers, Func<IEnumerable<IConsumer>, IEnumerable<IConsumer>> consumerFilter, CurrentMessageInformation message)
        {
            currentMessage = message;

            var consumers = resolveConsumers(currentMessage.MessageType);
            var autoConsumers = consumers.Item2.ToArray();

            foreach (var c in consumerFilter(consumers.Item1.Concat(autoConsumers)))
                reflection.InvokeConsume(c, currentMessage.Body);

            consumerResolver.Release(autoConsumers);
        }

        public IDisposable AddInstanceSubscription(IConsumer consumer)
        {
            var model = CreateModel();
            var queue = model.QueueDeclare("", false, true, true, null);

            var consumerReference = new WeakReference(consumer);
            instanceConsumers.TryAdd(consumerReference, model);

            var queueConsumer = Subscribe(model, GetSupportedMessageTypes(consumer), queue);

            ConsumeAsynchronously(consumer, queueConsumer);

            return RemoveSubscriptionAndDisposeModel(consumerReference);
        }

        private IDisposable RemoveSubscriptionAndDisposeModel(WeakReference consumerReference)
        {
            return new DisposableAction(() =>
            {
                IModel model;

                if (instanceConsumers.TryRemove(consumerReference, out model))
                    model.Dispose();
            });
        }

        private void ConsumeAsynchronously(IConsumer consumer, QueueingBasicConsumer queueConsumer)
        {
            ConsumeAsynchronously(_ => Tuple.Create(consumer.Return(), Enumerable.Empty<IConsumer>()), queueConsumer, Identity, Identity);
        }

        private static T Identity<T>(T value)
        {
            return value;
        }

        private IEnumerable<Type> GetSupportedMessageTypes(IConsumer consumer)
        {
            return supportedMessageTypesResolver.Get(consumer.GetType());
        }

        public void Publish(object message)
        {
            using (var model = CreateModel())
            {
                var messageType = message.GetType();
                var properties = PopulatePropertiesWithMessageType(model, messageType);

                model.BasicPublish(exchangeResolver.Resolve(messageType),
                                   routingKeyResolver.Resolve(messageType),
                                   properties,
                                   serializer.Serialize(message));
            }
        }

        private IBasicProperties PopulatePropertiesWithMessageType(IModel model, Type messageType)
        {
            var properties = model.CreateBasicProperties();
            properties.Type = typeResolver.Unresolve(messageType);
            return properties;
        }

        public void PublishMandatory(object message, Action<PublishFailureReason> publishFailure)
        {
            using (var model = CreateModel())
            {
                var messageType = message.GetType();
                var properties = PopulatePropertiesWithMessageType(model, messageType);

                PublishMandatoryInternal(message, properties, model, messageType, publishFailure);
                // disposing here, is this correct? what if BasicReturn needs to be invoked by a disposed model?
                // tests succeed, BTW
            }
        }

        private void PublishMandatoryInternal(object message,
                                              IBasicProperties properties,
                                              IModel model,
                                              Type messageType,
                                              Action<PublishFailureReason> publishFailure)
        {
            CallbackOnBasicReturn(model, publishFailure);

            model.BasicPublish(exchangeResolver.Resolve(messageType),
                               routingKeyResolver.Resolve(messageType),
                               true,
                               false,
                               properties,
                               serializer.Serialize(message));
        }

        public void Request(object message)
        {
            Request(message, _ => {}, _ => {});
        }

        public void Request(object message, Action<PublishFailureReason> requestFailure)
        {
            Request(message, requestFailure, _ => {});
        }

        public void Request(object message, Action<PublishFailureReason> requestFailure, Action<ReplyFailureReason> replyFailure)
        {
            var responseModel = CreateModel();
            var responseQueue = responseModel.QueueDeclare("", false, true, true, null);

            var responseConsumer = new QueueingBasicConsumer(responseModel);
            responseModel.BasicConsume(responseQueue, false, responseConsumer);

            SendRequest(responseQueue, message, requestFailure);

            ListenForResponseAsync(responseConsumer, replyFailure);
        }

        private void SendRequest(string responseQueue, object message, Action<PublishFailureReason> requestFailure)
        {
            using (var model = CreateModel())
            {
                var messageType = message.GetType();
                var properties = PopulatePropertiesWithMessageType(model, messageType);

                properties.CorrelationId = Guid.NewGuid().ToString();
                properties.ReplyTo = responseQueue;

                PublishMandatoryInternal(message, properties, model, messageType, requestFailure);
            }
        }

        private void ListenForResponseAsync(QueueingBasicConsumer consumer, Action<ReplyFailureReason> replyFailure)
        {
            ConsumeAsynchronously(ResolveConsumers, consumer, 
                                  messages => messages.First().Return(),
                                  consumers => consumers.Single().Return())
                .ContinueWith(t => replyFailure(new ReplyFailureReason(t.Exception)), TaskContinuationOptions.OnlyOnFaulted)
                .ContinueWith(_ => consumer.Model.Dispose());
        }

        public void Reply(object message)
        {
            if (CurrentMessage == null || string.IsNullOrWhiteSpace(CurrentMessage.ReplyTo))
                throw new InvalidOperationException(ErrorMessages.ReplyInvokedOutOfRequestContext);

            using (var model = CreateModel())
            {
                var messageType = message.GetType();
                var properties = PopulatePropertiesWithMessageType(model, messageType);
                properties.CorrelationId = CurrentMessage.CorrelationId;

                model.BasicPublish(DefaultExchange, CurrentMessage.ReplyTo, properties, serializer.Serialize(message));
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

        private static void CallbackOnBasicReturn(IModel model, Action<PublishFailureReason> publishFailure)
        {
            model.BasicReturn += (_, args) =>
            {
                try
                {
                    publishFailure(new PublishFailureReason(args.ReplyCode, args.ReplyText));
                }
                finally
                {
                    model.Dispose();
                }
            };
        }

        public void Dispose()
        {
            if(connection != null && connection.IsOpen)
                connection.Dispose();
        }

        public void Consume(object message)
        {
            InvokeConsumers(ResolveConsumers, Identity, new CurrentMessageInformation
            {
                Body = message,
                MessageType = message.GetType()
            });
        }
    }
}