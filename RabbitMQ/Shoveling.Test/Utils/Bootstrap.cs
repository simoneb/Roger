using Common;
using MbUnit.Framework;
using Shoveling.Test.Properties;

namespace Shoveling.Test.Utils
{
    [AssemblyFixture]
    public class Bootstrap
    {
        private TcpTrace secondaryClientEndpoint;
        public static RabbitMQBroker Broker { get; private set; }
        public static TcpTrace ShovelTcpProxy { get; private set; }

        [FixtureSetUp]
        public void TestFixtureSetup()
        {
            ShovelTcpProxy = new TcpTrace(@"..\..\..\..\tools\tcpTrace\tcpTrace.exe");
            StartShovelLink();

            secondaryClientEndpoint = new TcpTrace(@"..\..\..\..\tools\tcpTrace\tcpTrace.exe");
            secondaryClientEndpoint.Start(Globals.SecondaryPort, "localhost", Globals.Port, "Secondary client link");

            Broker = new RabbitMQBroker(@"..\..\..\..\RabbitMQServer");

            StartBroker();

            Broker.StopApp();
            Broker.Reset();
            Broker.StartAppAndWait();
        }

        public static void StartShovelLink()
        {
            ShovelTcpProxy.Start(Globals.ShovelPort, "localhost", Globals.Port, "Shovel");
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

            StopShovelLink();
            secondaryClientEndpoint.Stop();

            TcpTrace.StopAll(); // safety net
        }

        public static void StopShovelLink()
        {
            ShovelTcpProxy.Stop();
        }

        private static void StopBroker()
        {
            if (Settings.Default.RunEmbeddedBroker)
                Broker.Stop();
        }
    }
}