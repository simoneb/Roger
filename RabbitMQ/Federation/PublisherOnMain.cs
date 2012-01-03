using System;
using Common;
using RabbitMQ.Client;

namespace Federation
{
    [Serializable]
    public class PublisherOnMain : PublisherBase
    {
        protected override IConnection CreateConnection()
        {
            return Helpers.CreateConnection();
        }
    }
}