using System;
using Common;
using RabbitMQ.Client;

namespace Shoveling
{
    [Serializable]
    public class SubscriberOnSecondary : SubscriberBase
    {
        protected override IConnection CreateConnection()
        {
            return Helpers.CreateSecondaryConnection();
        }
    }
}