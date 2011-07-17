using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Rabbus
{
    public class DefaultBus
    {
        private readonly IConnection connection;
        private readonly IRoutingKeyGenerator routingKeyGenerator;
        private readonly ITypeNameGenerator typeNameGenerator;
        private readonly IMessageSerializer serializer;
        private IReflection reflection;

        public DefaultBus(IConnection connection, IRoutingKeyGenerator routingKeyGenerator, ITypeNameGenerator typeNameGenerator, IMessageSerializer serializer, IReflection reflection)
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

            foreach (var messageType in from i in consumer.GetType().GetInterfaces()
                                        where i.IsGenericType
                                        where typeof (IConsumer<>).IsAssignableFrom(i.GetGenericTypeDefinition())
                                        select i.GetGenericArguments().Single())
                model.QueueBind(queue, GetExchange(messageType), routingKeyGenerator.GetRoutingKey(messageType));

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
                var properties = FillMessageProperties(model, message);

                model.BasicPublish(GetExchange(message.GetType()), 
                                   routingKeyGenerator.GetRoutingKey(message.GetType()), 
                                   properties,
                                   serializer.Serialize(message));
            }
        }

        private IBasicProperties FillMessageProperties(IModel model, object message)
        {
            var properties = model.CreateBasicProperties();
            properties.Type = typeNameGenerator.GetName(message.GetType());
            return properties;
        }
    }
}