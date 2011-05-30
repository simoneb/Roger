using MbUnit.Framework;
using Shoveling.Test.Properties;

namespace Shoveling.Test.Utils
{
    [AssemblyFixture]
    public class Bootstrap
    {
        public static RabbitMQBroker Broker { get; private set; }

        [FixtureSetUp]
        public void TestFixtureSetup()
        {
            Broker = new RabbitMQBroker(@"..\..\..\..\RabbitMQServer");

            StartBroker();

            Broker.StopApp();
            Broker.Reset();
            Broker.StartAppAndWait();
        }

        private static void StartBroker()
        {
            if (Settings.Default.RunEmbeddedBroker)
                Broker.StartAndWait();
        }

        [FixtureTearDown]
        public void TestFixtureTeardown()
        {
            StopBroker();
        }

        private static void StopBroker()
        {
            if (Settings.Default.RunEmbeddedBroker)
                Broker.Stop();
        }
    }
}