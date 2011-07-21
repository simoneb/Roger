using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Rabbus.ConsumerToMessageType;
using Rabbus.Errors;
using Rabbus.Exchanges;
using Rabbus.Reflection;
using Rabbus.RoutingKeys;
using Rabbus.Serialization;
using Rabbus.TypeNames;
using Rabbus.Utilities;

namespace Rabbus
{
    public class DefaultBus : IRabbitBus
    {
        private readonly IConnection connection;
        private readonly IRoutingKeyGenerator routingKeyGenerator;
        private readonly ITypeResolver typeResolver;
        private readonly IMessageSerializer serializer;
        private readonly IReflection reflection;
        private readonly IConsumerResolver consumerResolver;
        private readonly IConsumerTypeToMessageTypes consumerTypeToMessageTypes;
        private readonly IExchangeResolver exchangeResolver;

        public CurrentMessageInformation CurrentMessage { get { return currentMessage; } }

        [ThreadStatic]
        private static CurrentMessageInformation currentMessage;

        private readonly ConcurrentDictionary<WeakReference, IModel> instanceConsumers = new ConcurrentDictionary<WeakReference, IModel>();
        private IModel mainModel;

        public DefaultBus(IConnectionFactory connectionFactory, IConsumerResolver consumerResolver) :
            this(connectionFactory,
                 new DefaultRoutingKeyGenerator(),
                 new DefaultTypeResolver(),
                 new ProtoBufNetSerializer(),
                 new DefaultReflection(),
                 consumerResolver,
                 new DefaultConsumerTypeToMessageTypes(),
                 new DefaultExchangeResolver())
        {
        }

        public DefaultBus(IConnectionFactory connectionFactory,
                          IRoutingKeyGenerator routingKeyGenerator,
                          ITypeResolver typeResolver,
                          IMessageSerializer serializer,
                          IReflection reflection,
                          IConsumerResolver consumerResolver,
                          IConsumerTypeToMessageTypes consumerTypeToMessageTypes,
                          IExchangeResolver exchangeResolver)
        {
            this.reflection = reflection;
            this.consumerResolver = consumerResolver;
            this.consumerTypeToMessageTypes = consumerTypeToMessageTypes;
            this.exchangeResolver = exchangeResolver;
            this.serializer = serializer;
            this.routingKeyGenerator = routingKeyGenerator;
            this.typeResolver = typeResolver;

            connection = connectionFactory.CreateConnection();
        }

        public void Reply(object message)
        {
            var model = CreateModel();

            var properties = PopulatePropertiesWithMessageType(model, message.GetType());
            properties.CorrelationId = CurrentMessage.CorrelationId;

            model.BasicPublish("", CurrentMessage.ReplyTo, properties, serializer.Serialize(message));
        }

        public void Initialize()
        {
            var allMessages = consumerResolver.GetAllConsumersTypes()
                .SelectMany(type => consumerTypeToMessageTypes.Get(type))
                .Distinct();

            mainModel = CreateModel();
            var queue = mainModel.QueueDeclare("", false, true, true, null);

            var consumer = Subscribe(mainModel, allMessages, queue);

            ConsumeAsynchronously(ResolveConsumers, consumer, Identity, Identity);
        }

        private IModel CreateModel()
        {
            return connection.CreateModel();
        }

        private QueueingBasicConsumer Subscribe(IModel model, IEnumerable<Type> messageTypes, string queue)
        {
            foreach (var messageType in messageTypes)
                model.QueueBind(queue, exchangeResolver.Resolve(messageType), routingKeyGenerator.Generate(messageType));

            var queueConsumer = new QueueingBasicConsumer(model);
            model.BasicConsume(queue, false, queueConsumer);
            return queueConsumer;
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
            foreach (var message in from args in messageFilter(consumer.Queue.OfType<BasicDeliverEventArgs>())
                                    let messageType = typeResolver.ResolveType(args.BasicProperties.Type)
                                    let replyTo = args.BasicProperties.ReplyTo
                                    let correlationId = args.BasicProperties.ReplyTo
                                    let deliveryTag = args.DeliveryTag
                                    select new CurrentMessageInformation
                                           {
                                               MessageType = messageType,
                                               ReplyTo = replyTo,
                                               CorrelationId = correlationId,
                                               DeliveryTag = deliveryTag,
                                               Body = serializer.Deserialize(messageType, args.Body)
                                           })
            {
                InvokeConsumers(resolveConsumers, consumerFilter, message);
                consumer.Model.BasicAck(currentMessage.DeliveryTag, false);
            }
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
            return consumerTypeToMessageTypes.Get(consumer.GetType());
        }

        public void Publish(object message)
        {
            using (var model = CreateModel())
            {
                var messageType = message.GetType();
                var properties = PopulatePropertiesWithMessageType(model, messageType);

                model.BasicPublish(exchangeResolver.Resolve(messageType),
                                   routingKeyGenerator.Generate(messageType),
                                   properties,
                                   serializer.Serialize(message));
            }
        }

        private IBasicProperties PopulatePropertiesWithMessageType(IModel model, Type messageType)
        {
            var properties = model.CreateBasicProperties();
            properties.Type = typeResolver.GenerateTypeName(messageType);
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
                               routingKeyGenerator.Generate(messageType),
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

        public void Request(object message, Action<ReplyFailureReason> replyFailure)
        {
            Request(message, _ => { }, replyFailure);
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