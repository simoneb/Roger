using System;
using System.Text;
using System.Threading;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tutorial6
{
    [Serializable]
    public class FibonacciService : IProcess
    {
        public void Start(WaitHandle waitHandle)
        {
            using (var connection = Helpers.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.QueueDeclare(Constants.RpcQueueName, false, false, true, null);
                channel.BasicQos(0, 1, false);

                var consumer = new EventingBasicConsumer {Model = channel};
                consumer.Received += ConsumerOnReceived;
                channel.BasicConsume(Constants.RpcQueueName, false, consumer);

                waitHandle.WaitOne();
            }
        }

        private static void ConsumerOnReceived(IBasicConsumer sender, BasicDeliverEventArgs args)
        {
            var n = int.Parse(Encoding.UTF8.GetString(args.Body));

            Console.WriteLine("Received computation request for n = {0}, correlation id {1}", n, args.BasicProperties.CorrelationId);

            var fib = Fibonacci(n);

            var properties = sender.Model.CreateBasicProperties();
            properties.CorrelationId = args.BasicProperties.CorrelationId;

            sender.Model.BasicPublish("", args.BasicProperties.ReplyTo, properties, Encoding.UTF8.GetBytes(fib.ToString()));

            sender.Model.BasicAck(args.DeliveryTag, false);
        }

        private static int Fibonacci(int n)
        {
            return n <= 1 ? n : Fibonacci(n - 1) + Fibonacci(n - 2);
        }
    }
}