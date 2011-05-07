using System;
using Common;
using RabbitMQ.Client;

namespace Shoveling
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