using System.Configuration;
using Common;
using MbUnit.Framework;

namespace Tests.Utils
{
    [AssemblyFixture]
    public class Bootstrap
    {
        private TcpTrace secondaryClientEndpoint;
        public static RabbitMQBroker Broker { get; private set; }
        public static TcpTrace ShovelTcpProxy { get; private set; }
        public static bool RunEmbeddedBroker { get { return bool.Parse(ConfigurationManager.AppSettings["RunEmbeddedBroker"]); } }

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
            if (RunEmbeddedBroker)
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
            if (RunEmbeddedBroker)
                Broker.Stop();
        }
    }
}