using System;
using System.Text;
using System.Threading;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tutorial1
{
    [Serializable]
    public class Receiver : IProcess
    {
        public void Start(WaitHandle waitHandle)
        {
            using (var connection = Helpers.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare("hello", false, false, false, null);
                Console.WriteLine("Waiting for messages...");

                var consumer = new EventingBasicConsumer();
                consumer.Received += ConsumerOnReceived;
                channel.BasicConsume(Constants.QueueName, true, consumer);

                waitHandle.WaitOne();
            }
        }

        private static void ConsumerOnReceived(IBasicConsumer sender, BasicDeliverEventArgs args)
        {
            Console.WriteLine("Received {0}", Encoding.UTF8.GetString(args.Body));
        }
    }
}