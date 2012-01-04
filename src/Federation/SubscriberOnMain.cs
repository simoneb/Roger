using System;
using Common;
using RabbitMQ.Client;

namespace Federation
{
    [Serializable]
    public class SubscriberOnMain : SubscriberBase
    {
        protected override IConnection CreateConnection()
        {
            return Helpers.CreateConnection();
        }
    }
}