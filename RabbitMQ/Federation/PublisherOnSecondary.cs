using System;
using Common;
using RabbitMQ.Client;

namespace Federation
{
    [Serializable]
    public class PublisherOnSecondary : PublisherBase
    {
        protected override IConnection CreateConnection()
        {
            return Helpers.CreateSecondaryConnection();
        }
    }
}