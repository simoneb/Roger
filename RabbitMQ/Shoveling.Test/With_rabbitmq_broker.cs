using Common;
using Resbit;
using Shoveling.Test.Utils;

namespace Shoveling.Test
{
    public class With_rabbitmq_broker
    {
        protected static RabbitMQBroker Broker { get { return Bootstrap.Broker;  } }
        protected static readonly ResbitClient Client = new ResbitClient(Globals.HostName, "guest", "guest");
    }
}