using System;
using System.Threading;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Shoveling
{
    [Serializable]
    public abstract class SubscriberBase : IProcess
    {
        public void Start(WaitHandle waitHandle)
        {
            using (var connection = CreateConnection())
            using (var model = connection.CreateModel())
            {
                var queue = model.QueueDeclare("", false, true, false, null);

                model.QueueBind(queue, Constants.ExchangeName, "Routing.Key");

                var consumer = new EventingBasicConsumer { Model = model };

                consumer.Received += ConsumerOnReceived;

                model.BasicConsume(queue, true, consumer);

                waitHandle.WaitOne();
            }
        }

        protected abstract IConnection CreateConnection();

        private static void ConsumerOnReceived(IBasicConsumer sender, BasicDeliverEventArgs args)
        {
            Console.WriteLine("Message received: {0}", args.Body.String());            
        }
    }
}