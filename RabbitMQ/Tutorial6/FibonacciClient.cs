using System;
using System.Text;
using System.Threading;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tutorial6
{
    [Serializable]
    public class FibonacciClient : IProcess
    {
        private readonly int n;
        private int correlationId = 0;

        public FibonacciClient(int n)
        {
            this.n = n;
        }

        public void Start(WaitHandle waitHandle)
        {
             using (var connection = new ConnectionFactory {HostName = Globals.HostName}.CreateConnection())
             using (var channel = connection.CreateModel())
             {
                 channel.QueueDeclare(Constants.RpcQueueName, false, false, true, null);
                 var queueName = channel.QueueDeclare("", false, true, true, null);

                 var consumer = new EventingBasicConsumer {Model = channel};
                 consumer.Received += ConsumerOnReceived;

                 channel.BasicConsume(queueName, true, consumer);

                 while (!waitHandle.WaitOne(TimeSpan.FromSeconds(5)))
                     CallService(channel, queueName);
             }
        }

        private void CallService(IModel channel, string queueName)
        {
            var properties = channel.CreateBasicProperties();
            properties.ReplyTo = queueName;
            properties.CorrelationId = (++correlationId).ToString();
            channel.BasicPublish("", Constants.RpcQueueName, properties, Encoding.UTF8.GetBytes(n.ToString()));
        }

        private static void ConsumerOnReceived(IBasicConsumer sender, BasicDeliverEventArgs args)
        {
            Console.WriteLine("Fibonacci result: {0}, for correlation id {1}", Encoding.UTF8.GetString(args.Body), args.BasicProperties.CorrelationId);
        }
    }
}