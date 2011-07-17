using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ProtoBuf;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Rabbus
{
    public class DefaultBus
    {
        private readonly IConnection connection;
        private readonly IRoutingKeyGenerationStrategy routingKeyGenerationStrategy;
        private readonly ITypeNameGenerationStrategy typeNameGenerationStrategy;

        public DefaultBus(IConnection connection, IRoutingKeyGenerationStrategy routingKeyGenerationStrategy, ITypeNameGenerationStrategy typeNameGenerationStrategy)
        {
            this.connection = connection;
            this.routingKeyGenerationStrategy = routingKeyGenerationStrategy;
            this.typeNameGenerationStrategy = typeNameGenerationStrategy;
        }

        public IDisposable Subscribe(IRabbusConsumer consumer)
        {
            var model = connection.CreateModel();
            var queue = model.QueueDeclare("", false, true, true, null);

            var messageTypes = consumer.GetType().GetInterfaces()
                                                 .Where(i => i.IsGenericType)
                                                 .Where(i => typeof (IConsumer<>).IsAssignableFrom(i.GetGenericTypeDefinition()))
                                                 .Select(c => c.GetGenericArguments().Single());

            foreach (var messageType in messageTypes)
            {
                if(!messageType.IsDefined(typeof(RabbusMessageAttribute), true))
                    throw new InvalidOperationException(string.Format("Message {0} should be decorated with {1} attribute", messageType.FullName, typeof(RabbusMessageAttribute).FullName));

                var exchange = messageType.Attribute<RabbusMessageAttribute>().Exchange;

                if(exchange != "")
                    model.QueueBind(queue, exchange, routingKeyGenerationStrategy.GetRoutingKey(messageType));
            }

            var queueConsumer = new QueueingBasicConsumer(model);

            model.BasicConsume(queue, true, queueConsumer);

            Task.Factory.StartNew(() =>
                                  {
                                      foreach (var args in queueConsumer.Queue.OfType<BasicDeliverEventArgs>())
                                      {
                                          var messageType = Type.GetType(args.BasicProperties.Type, true);

                                          var deserialize = typeof (Serializer).GetMethod("Deserialize").MakeGenericMethod(messageType);

                                          object message;

                                          using (var s = new MemoryStream(args.Body))
                                              message = deserialize.Invoke(typeof (Serializer), new[] {s});

                                          consumer.GetType().InvokeMember("Consume", BindingFlags.InvokeMethod, null,
                                                                          consumer, new[] {message});
                                      }
                                  });

            return new DisposableAction(model.Dispose);
        }
    }
}