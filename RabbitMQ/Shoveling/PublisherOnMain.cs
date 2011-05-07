using System;
using Common;
using RabbitMQ.Client;

namespace Shoveling
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