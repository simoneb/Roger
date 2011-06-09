using Common;
using MbUnit.Framework;
using Shoveling.Test.Properties;

namespace Shoveling.Test.Utils
{
    [AssemblyFixture]
    public class Bootstrap
    {
        public static RabbitMQBroker Broker { get; private set; }
        private TcpTrace TcpProxy { get; set; }

        [FixtureSetUp]
        public void TestFixtureSetup()
        {
            TcpProxy = new TcpTrace(@"..\..\..\..\tools\tcpTrace\tcpTrace.exe");
            TcpProxy.Start(Globals.SecondaryPort, "localhost", Globals.Port);

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

            TcpProxy.Stop();
            TcpTrace.StopAll(); // safety net
        }

        private static void StopBroker()
        {
            if (Settings.Default.RunEmbeddedBroker)
                Broker.Stop();
        }
    }
}