using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Rabbus
{
    public class DefaultBus : IRabbitBus
    {
        private readonly IConnection connection;
        private readonly IRoutingKeyGenerator routingKeyGenerator;
        private readonly ITypeNameGenerator typeNameGenerator;
        private readonly IMessageSerializer serializer;
        private readonly IReflection reflection;
        private readonly IConsumerResolver consumerResolver;
        private readonly IConsumerTypeToMessageTypes consumerTypeToMessageTypes;

        [ThreadStatic]
        private static CurrentMessageInformation currentMessage;

        private readonly ConcurrentDictionary<WeakReference, IModel> instanceConsumers = new ConcurrentDictionary<WeakReference, IModel>();

        public void Reply(object message)
        {
            var model = connection.CreateModel();

            var properties = PopulatePropertiesWithMessageType(model, message.GetType());
            properties.CorrelationId = CurrentMessage.CorrelationId;

            model.BasicPublish("", CurrentMessage.ReplyTo, properties, serializer.Serialize(message));
        }

        public CurrentMessageInformation CurrentMessage { get { return currentMessage; } }

        public DefaultBus(IConnection connection,
                          IRoutingKeyGenerator routingKeyGenerator,
                          ITypeNameGenerator typeNameGenerator,
                          IMessageSerializer serializer,
                          IReflection reflection,
                          IConsumerResolver consumerResolver,
                          IConsumerTypeToMessageTypes consumerTypeToMessageTypes)
        {
            this.connection = connection;
            this.reflection = reflection;
            this.consumerResolver = consumerResolver;
            this.consumerTypeToMessageTypes = consumerTypeToMessageTypes;
            this.serializer = serializer;
            this.routingKeyGenerator = routingKeyGenerator;
            this.typeNameGenerator = typeNameGenerator;
        }

        public IDisposable AddInstanceSubscription(IConsumer consumer)
        {
            var model = connection.CreateModel();
            var queue = model.QueueDeclare("", false, true, true, null);

            var consumerReference = new WeakReference(consumer);
            instanceConsumers.TryAdd(consumerReference, model);

            var queueConsumer = Subscribe(model, GetSupportedMessageTypes(consumer), queue);

            ConsumeAsynchronously(consumer, queueConsumer);

            return RemoveSubscriptionAndDisposeModel(consumerReference);
        }

        private QueueingBasicConsumer Subscribe(IModel model, IEnumerable<Type> messageTypes, string queue)
        {
            foreach (var messageType in messageTypes)
                model.QueueBind(queue, GetMessageExchange(messageType), routingKeyGenerator.Generate(messageType));

            var queueConsumer = new QueueingBasicConsumer(model);
            model.BasicConsume(queue, true, queueConsumer);
            return queueConsumer;
        }

        public void Initialize()
        {
            var allMessages = consumerResolver.GetAllConsumersTypes()
                .SelectMany(type => consumerTypeToMessageTypes.Get(type))
                .Distinct();

            var model = connection.CreateModel();
            var queue = model.QueueDeclare("", false, true, true, null);

            var consumer = Subscribe(model, allMessages, queue);

            ConsumeAsynchronously(ResolveConsumers, consumer, Identity, Identity);
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

        private Task ConsumeAsynchronously(Func<Type, Tuple<IEnumerable<IConsumer>, IEnumerable<IConsumer>>> resolveConsumers,
                                           QueueingBasicConsumer queueConsumer,
                                           Func<IEnumerable<BasicDeliverEventArgs>, IEnumerable<BasicDeliverEventArgs>> messageFilter, 
                                           Func<IEnumerable<IConsumer>, IEnumerable<IConsumer>> consumerFilter)
        {
            return Task.Factory.StartNew(() => ConsumeSynchronously(resolveConsumers, queueConsumer, messageFilter, consumerFilter));
        }

        private void ConsumeSynchronously(Func<Type, Tuple<IEnumerable<IConsumer>, IEnumerable<IConsumer>>> resolveConsumers,
                                          QueueingBasicConsumer queueConsumer,
                                          Func<IEnumerable<BasicDeliverEventArgs>, IEnumerable<BasicDeliverEventArgs>> messageFilter, 
                                          Func<IEnumerable<IConsumer>, IEnumerable<IConsumer>> consumerFilter)
        {
            foreach (var message in from args in messageFilter(queueConsumer.Queue.OfType<BasicDeliverEventArgs>())
                                    let messageType = Type.GetType(args.BasicProperties.Type, true)
                                    let replyTo = args.BasicProperties.ReplyTo
                                    let correlationId = args.BasicProperties.ReplyTo
                                    select new
                                           {
                                               messageType,
                                               replyTo,
                                               correlationId,
                                               body = serializer.Deserialize(messageType, args.Body)
                                           })
            {
                currentMessage = new CurrentMessageInformation
                                 {
                                     ReplyTo = message.replyTo,
                                     CorrelationId = message.correlationId,
                                     MessageType = message.messageType
                                 };

                var consumers = resolveConsumers(currentMessage.MessageType);
                var autoConsumers = consumers.Item2;

                foreach (var consumer in consumerFilter(consumers.Item1.Concat(autoConsumers)))
                    reflection.InvokeConsume(consumer, message.body);

                consumerResolver.Release(autoConsumers);
            }
        }

        private IEnumerable<Type> GetSupportedMessageTypes(IConsumer consumer)
        {
            return consumerTypeToMessageTypes.Get(consumer.GetType());
        }

        private static string GetMessageExchange(Type messageType)
        {
            EnsureCorrectMessageType(messageType);

            var exchange = messageType.Attribute<RabbusMessageAttribute>().Exchange;

            if(string.IsNullOrWhiteSpace(exchange))
                throw new InvalidOperationException(string.Format(@"Message type {0} should have a valid exchange name where it should be published.
It can be specified using the {1} attribute", messageType.FullName, typeof(RabbusMessageAttribute).FullName));

            return exchange;
        }

        private static void EnsureCorrectMessageType(Type messageType)
        {
            if (!messageType.IsDefined(typeof(RabbusMessageAttribute), true))
                throw new InvalidOperationException(string.Format("Message type {0} should be decorated with {1} attribute",
                                                                  messageType.FullName,
                                                                  typeof(RabbusMessageAttribute).FullName));

        }

        public void Publish(object message)
        {
            using (var model = connection.CreateModel())
            {
                var messageType = message.GetType();
                var properties = PopulatePropertiesWithMessageType(model, messageType);

                model.BasicPublish(GetMessageExchange(messageType),
                                   routingKeyGenerator.Generate(messageType),
                                   properties,
                                   serializer.Serialize(message));
            }
        }

        private IBasicProperties PopulatePropertiesWithMessageType(IModel model, Type messageType)
        {
            var properties = model.CreateBasicProperties();
            properties.Type = typeNameGenerator.Generate(messageType);
            return properties;
        }

        public void PublishMandatory(object message, Action<PublishFailureReason> publishFailure)
        {
            using (var model = connection.CreateModel())
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

            model.BasicPublish(GetMessageExchange(messageType),
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
            var responseModel = connection.CreateModel();
            var responseQueue = responseModel.QueueDeclare("", false, true, true, null);

            var responseConsumer = new QueueingBasicConsumer(responseModel);
            responseModel.BasicConsume(responseQueue, true, responseConsumer);

            SendRequest(responseQueue, message, requestFailure);

            ListenForResponseAsync(responseConsumer, replyFailure);
        }

        private void SendRequest(string responseQueue, object message, Action<PublishFailureReason> requestFailure)
        {
            using (var model = connection.CreateModel())
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
    }
}