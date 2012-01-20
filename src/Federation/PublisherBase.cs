using System;
using System.Threading;
using Common;
using RabbitMQ.Client;

namespace Federation
{
    [Serializable]
    public abstract class PublisherBase : IProcess
    {
        public void Start(WaitHandle waitHandle)
        {
            using (var connection = CreateConnection())
            using (var model = connection.CreateModel())
            {
                while (!waitHandle.WaitOne(TimeSpan.FromMilliseconds(100)))
                {
                    model.BasicPublish(Constants.FederationExchangeName, "Routing.Key", null, string.Format("Ciao from {0}", GetType().Name).Bytes());
                    Console.WriteLine("Message published");
                }
            }
        }

        protected abstract IConnection CreateConnection();
    }
}