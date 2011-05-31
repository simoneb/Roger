using System.Threading;
using MbUnit.Framework;

namespace Shoveling.Test
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
        }

        [FixtureTearDown]
        public void DeleteSecondaryVHost()
        {
            Broker.DeleteVHost("secondary");
        }
    }
}