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
        private readonly string[] severities;

        public Consumer(params string[] severities)
        {
            this.severities = severities;
        }

        public void Start(WaitHandle waitHandle)
        {
            var connectionFactory = new ConnectionFactory { HostName = Globals.HostName };

            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(Constants.ExchangeName, ExchangeType.Direct);
                var queue = channel.QueueDeclare("", false, true, true, null);

                foreach (var severity in severities)
                    channel.QueueBind(queue, Constants.ExchangeName, severity);

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
            return base.ToString() + " " + string.Join(", ", severities);
        }
    }
}