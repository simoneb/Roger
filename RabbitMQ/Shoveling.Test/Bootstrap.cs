using MbUnit.Framework;

namespace Shoveling.Test
{
    [AssemblyFixture]
    public class Bootstrap
    {
        public static RabbitMQBroker Broker { get; private set; }

        [FixtureSetUp]
        public void TestFixtureSetup()
        {
            Broker = new RabbitMQBroker(@"..\..\..\..\RabbitMQServer");
            Broker.StartAndWait();
            Broker.StopApp();
            Broker.Reset();
            Broker.StartAppAndWait();
        }

        [FixtureTearDown]
        public void TestFixtureTeardown()
        {
            Broker.Stop();
        }
    }
}