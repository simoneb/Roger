using System;
using System.Text;
using System.Threading;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tutorial4
{
    [Serializable]
    public class Consumer : IProcess
    {
        private readonly string[] bindingKeys;

        public Consumer(params string[] bindingKeys)
        {
            this.bindingKeys = bindingKeys;
        }

        public void Start(WaitHandle waitHandle)
        {
            var connectionFactory = new ConnectionFactory { HostName = Globals.HostName };

            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(Constants.ExchangeName, ExchangeType.Direct, false, true, null);
                var queue = channel.QueueDeclare("", false, true, true, null);

                foreach (var bindingKey in bindingKeys)
                    channel.QueueBind(queue, Constants.ExchangeName, bindingKey);

                var consumer = new EventingBasicConsumer {Model = channel};

                consumer.Received += ConsumerOnReceived;

                channel.BasicConsume(queue, true, consumer);

                waitHandle.WaitOne();
            }
        }

        private static void ConsumerOnReceived(IBasicConsumer sender, BasicDeliverEventArgs args)
        {
            Console.WriteLine("Message received: {0}", Encoding.UTF8.GetString(args.Body));
        }

        public override string ToString()
        {
            return base.ToString() + " " + string.Join(", ", bindingKeys);
        }
    }
}