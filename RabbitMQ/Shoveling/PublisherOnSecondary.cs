using System;
using Common;
using RabbitMQ.Client;

namespace Shoveling
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