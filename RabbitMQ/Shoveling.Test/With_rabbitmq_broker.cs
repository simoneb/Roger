using System;

namespace Shoveling.Test
{
    public class With_rabbitmq_broker
    {
        protected static RabbitMQBroker Broker { get { return Bootstrap.Broker;  } }
    }
}