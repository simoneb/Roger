using System;
using Common;
using RabbitMQ.Client;

namespace Federation
{
    [Serializable]
    public class SubscriberOnSecondary : SubscriberBase
    {
        protected override IConnection CreateConnection()
        {
            return Helpers.CreateConnectionToSecondaryVirtualHostOnAlternativePort();
        }
    }
}