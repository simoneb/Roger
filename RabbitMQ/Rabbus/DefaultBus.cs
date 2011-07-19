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

        [ThreadStatic]
        private static CurrentMessageInformation _currentMessage;

        private readonly ConcurrentDictionary<WeakReference, IModel> instanceConsumers = new ConcurrentDictionary<WeakReference, IModel>();

        public void Reply(object message)
        {
            var model = connection.CreateModel();

            var properties = FillMessageProperties(model, message.GetType());
            properties.CorrelationId = CurrentMessage.CorrelationId;

            model.BasicPublish("", CurrentMessage.ReplyTo, properties, serializer.Serialize(message));
        }

        public CurrentMessageInformation CurrentMessage { get { return _currentMessage; } }

        public DefaultBus(IConnection connection, IRoutingKeyGenerator routingKeyGenerator,
                          ITypeNameGenerator typeNameGenerator, IMessageSerializer serializer, IReflection reflection)
        {
            this.connection = connection;
            this.reflection = reflection;
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

            foreach (var messageType in GetSupportedMessageTypes(consumer))
                model.QueueBind(queue, GetExchange(messageType), routingKeyGenerator.Generate(messageType));

            var queueConsumer = new QueueingBasicConsumer(model);
            model.BasicConsume(queue, true, queueConsumer);

            ConsumeAsynchronously(consumer, queueConsumer);

            return RemoveSubscriptionAndDisposeModel(consumerReference);
        }

        private IDisposable RemoveSubscriptionAndDisposeModel(WeakReference consumerReference)
        {
            return new DisposableAction(() =>
            {
                IModel model;

                if(instanceConsumers.TryRemove(consumerReference, out model))
                    model.Dispose();
            });
        }

        private void ConsumeAsynchronously(IConsumer consumer, QueueingBasicConsumer queueConsumer)
        {
            RunConsumeTask(_ => consumer.Return(), queueConsumer, Identity);
        }

        private static T Identity<T>(T value) { return value; }

        private Task RunConsumeTask(Func<Type, IEnumerable<IConsumer>> resolveConsumers,
                                    QueueingBasicConsumer queueConsumer,
                                    Func<IEnumerable<BasicDeliverEventArgs>, 
                                    IEnumerable<BasicDeliverEventArgs>> queryModifier)
        {
            return Task.Factory.StartNew(() =>
            {
                foreach (var message in from args in queryModifier(queueConsumer.Queue.OfType<BasicDeliverEventArgs>())
                                        let messageType = Type.GetType(args.BasicProperties.Type, true)
                                        let replyTo = args.BasicProperties.ReplyTo
                                        let correlationId = args.BasicProperties.ReplyTo
                                        select new { messageType, replyTo, correlationId, body = serializer.Deserialize(messageType, args.Body)})
                {
                    _currentMessage = new CurrentMessageInformation
                    {
                        ReplyTo = message.replyTo,
                        CorrelationId = message.correlationId,
                        MessageType = message.messageType
                    };

                    foreach (var consumer in resolveConsumers(_currentMessage.MessageType))
                        reflection.InvokeConsume(consumer, message.body);
                }
            });
        }

        private static IEnumerable<Type> GetSupportedMessageTypes(IConsumer consumer)
        {
            return from i in consumer.GetType().GetInterfaces()
                   where i.IsGenericType
                   where typeof (IConsumer<>).IsAssignableFrom(i.GetGenericTypeDefinition())
                   select i.GetGenericArguments().Single();
        }

        private static string GetExchange(Type messageType)
        {
            EnsureCorrectMessageType(messageType);

            return messageType.Attribute<RabbusMessageAttribute>().Exchange;
        }

        private static void EnsureCorrectMessageType(Type messageType)
        {
            if(!messageType.IsDefined(typeof(RabbusMessageAttribute), true))
                throw new InvalidOperationException(string.Format("Message {0} should be decorated with {1} attribute", messageType.FullName, typeof(RabbusMessageAttribute).FullName));
        }

        public void Publish(object message)
        {
            using(var model = connection.CreateModel())
            {
                var messageType = message.GetType();
                var properties = FillMessageProperties(model, messageType);

                model.BasicPublish(GetExchange(messageType), 
                                   routingKeyGenerator.Generate(messageType), 
                                   properties,
                                   serializer.Serialize(message));
            }
        }

        private IBasicProperties FillMessageProperties(IModel model, Type messageType)
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
                var properties = FillMessageProperties(model, messageType);

                PublishMandatoryInternal(message, properties, model, messageType, publishFailure);

                // disposing here, is this correct? what if BasicReturn needs to be invoked by a disposed model?
                // tests succeed, BTW
            }
        }

        private void PublishMandatoryInternal(object message, IBasicProperties properties, IModel model, Type messageType, Action<PublishFailureReason> publishFailure)
        {
            CallbackOnBasicReturn(model, publishFailure);

            model.BasicPublish(GetExchange(messageType),
                               routingKeyGenerator.Generate(messageType),
                               true,
                               false,
                               properties,
                               serializer.Serialize(message));
        }

        public void Request(object message)
        {
            var responseModel = connection.CreateModel();
            var responseQueue = responseModel.QueueDeclare("", false, true, true, null);
            
            var responseConsumer = new QueueingBasicConsumer(responseModel);
            responseModel.BasicConsume(responseQueue, true, responseConsumer);

            using (var requestModel = connection.CreateModel())
            {
                var messageType = message.GetType();

                var properties = FillMessageProperties(requestModel, messageType);
                properties.CorrelationId = Guid.NewGuid().ToString();
                properties.ReplyTo = responseQueue;

                PublishMandatoryInternal(message, properties, requestModel, messageType, _ => {});
            }

            RunConsumeTask(GetConsumers, responseConsumer, m => m.First().Return())
                .ContinueWith(_ => responseModel.Dispose());
        }

        private IEnumerable<IConsumer> GetConsumers(Type messageType)
        {
            return instanceConsumers.Keys.Where(r => r.IsAlive)
                .Select(r => r.Target)
                .Cast<IConsumer>()
                .Where(c => GetSupportedMessageTypes(c).Any(m => m.Equals(messageType)));
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