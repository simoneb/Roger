using System;
using System.Text;
using System.Threading;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tutorial5
{
    [Serializable]
    public class Consumer : IProcess
    {
        private readonly string bindingKey;

        public Consumer(string bindingKey)
        {
            this.bindingKey = bindingKey;
        }

        public void Start(WaitHandle waitHandle)
        {
            using (var connection = Helpers.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(Constants.ExchangeName, ExchangeType.Topic, false, true, null);
                var queue = channel.QueueDeclare("", false, true, true, null);

                channel.QueueBind(queue, Constants.ExchangeName, bindingKey);

                var consumer = new EventingBasicConsumer { Model = channel };

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
            return base.ToString() + " " + bindingKey;
        }
    }
}