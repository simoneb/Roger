using System;
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

        public DefaultBus(IConnection connection, IRoutingKeyGenerator routingKeyGenerator,
                          ITypeNameGenerator typeNameGenerator, IMessageSerializer serializer, IReflection reflection)
        {
            this.connection = connection;
            this.reflection = reflection;
            this.serializer = serializer;
            this.routingKeyGenerator = routingKeyGenerator;
            this.typeNameGenerator = typeNameGenerator;
        }

        public IDisposable Subscribe(IConsumer consumer)
        {
            var model = connection.CreateModel();
            var queue = model.QueueDeclare("", false, true, true, null);

            foreach (var messageType in GetMessageTypesFromConsumer(consumer))
                model.QueueBind(queue, GetExchange(messageType), routingKeyGenerator.Generate(messageType));

            var queueConsumer = new QueueingBasicConsumer(model);
            model.BasicConsume(queue, true, queueConsumer);

            Task.Factory.StartNew(() =>
                                  {
                                      foreach (var message in from args in queueConsumer.Queue.OfType<BasicDeliverEventArgs>()
                                                              let messageType = Type.GetType(args.BasicProperties.Type, true)
                                                              select serializer.Deserialize(messageType, args.Body))
                                          reflection.InvokeConsume(consumer, message);
                                  });

            return new DisposableAction(model.Dispose);
        }

        private static IEnumerable<Type> GetMessageTypesFromConsumer(IConsumer consumer)
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
                CallbackOnBasicReturn(model, publishFailure);

                var messageType = message.GetType();
                var properties = FillMessageProperties(model, messageType);

                model.BasicPublish(GetExchange(messageType),
                                   routingKeyGenerator.Generate(messageType),
                                   true,
                                   false,
                                   properties,
                                   serializer.Serialize(message));

                // disposing here, is this correct? what if BasicReturn needs to be invoked by a disposed model?
                // tests succeed, BTW
            }
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