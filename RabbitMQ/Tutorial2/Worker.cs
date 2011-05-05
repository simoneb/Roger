using System;
using System.Text;
using System.Threading;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tutorial2
{
    [Serializable]
    public class Worker : IProcess
    {
        readonly Random m_rnd = new Random();

        public void Start(WaitHandle waitHandle)
        {
            using (var connection = Helpers.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(Constants.QueueName, true, false, false, null);
                channel.BasicQos(0, 1, false);

                Console.WriteLine("Waiting for messages");

                var consumer = new EventingBasicConsumer {Model = channel};
                consumer.Received += ConsumerOnReceived;
                channel.BasicConsume(Constants.QueueName, false, consumer);

                waitHandle.WaitOne();
            }
        }

        private void ConsumerOnReceived(IBasicConsumer sender, BasicDeliverEventArgs args)
        {
            var millisecondsTimeout = m_rnd.Next(1, 10) * 1000;
            Console.WriteLine("Message received: {0}, sleeping for {1} seconds", Encoding.UTF8.GetString(args.Body), millisecondsTimeout/1000);

            Thread.Sleep(millisecondsTimeout);

            sender.Model.BasicAck(args.DeliveryTag, false);
        }
    }
}