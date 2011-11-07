using System.Threading;
using MbUnit.Framework;
using Tests.Integration.Utils;

namespace Tests.Integration
{
    public class With_shovel : With_rabbitmq_broker
    {
        [FixtureSetUp]
        public void SetupSecondaryVHost()
        {
            Broker.AddVHost("secondary");
            Broker.AddPermissions("secondary", "guest");
            Broker.StopApp();
            Thread.Sleep(1000);
            Broker.StartAppAndWait();

            // leave some time to TcpTrace to reestabilish the connection to the server
            Thread.Sleep(2000);
        }

        [FixtureTearDown]
        public void DeleteSecondaryVHost()
        {
            Broker.DeleteVHost("secondary");
        }

        protected static void StartShovelLink()
        {
            Bootstrap.StartShovelLink();
        }

        protected static void ShutdownShovelLink()
        {
            Bootstrap.StopShovelLink();
        }
    }
}