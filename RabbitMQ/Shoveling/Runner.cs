using System;
using System.Collections.Generic;
using System.Threading;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Shoveling
{
    public class Runner : IProcessesProvider
    {
        public IEnumerable<IProcess> Processes
        {
            get
            {
                yield return new PublisherOnMain();
                yield return new PublisherOnSecondary();
                yield return new SubscriberOnMain();
                yield return new SubscriberOnSecondary();
            }
        }
    }

    [Serializable]
    public class SubscriberOnMain : SubscriberBase
    {
        protected override IConnection CreateConnection()
        {
            return Helpers.CreateConnection();
        }
    }

    [Serializable]
    public abstract class SubscriberBase : IProcess
    {
        public void Start(WaitHandle waitHandle)
        {
            using (var connection = CreateConnection())
            using (var model = connection.CreateModel())
            {
                model.ExchangeDeclare(Constants.ExchangeName, ExchangeType.Topic);

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

    [Serializable]
    public class SubscriberOnSecondary : SubscriberBase
    {
        protected override IConnection CreateConnection()
        {
            return Helpers.CreateSecondaryConnection();
        }
    }

    [Serializable]
    public abstract class PublisherBase : IProcess
    {
        public void Start(WaitHandle waitHandle)
        {
            using (var connection = CreateConnection())
            using (var model = connection.CreateModel())
            {
                model.ExchangeDeclare(Constants.ExchangeName, ExchangeType.Topic);

                while (!waitHandle.WaitOne(TimeSpan.FromSeconds(5)))
                {
                    model.BasicPublish(Constants.ExchangeName, "Routing.Key", null, string.Format("Ciao from {0}", GetType().Name).Bytes());
                    Console.WriteLine("Message published");
                }
            }
        }

        protected abstract IConnection CreateConnection();
    }

    [Serializable]
    public class PublisherOnSecondary : PublisherBase
    {
        protected override IConnection CreateConnection()
        {
            return Helpers.CreateSecondaryConnection();
        }
    }

    [Serializable]
    public class PublisherOnMain : PublisherBase
    {
        protected override IConnection CreateConnection()
        {
            return Helpers.CreateConnection();
        }
    }
}
