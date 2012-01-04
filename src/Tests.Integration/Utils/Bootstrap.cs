using System.Collections;
using System.Configuration;
using System.Net;
using Common;
using MbUnit.Framework;
using Resbit;

namespace Tests.Integration.Utils
{
    [AssemblyFixture]
    public class Bootstrap
    {
        private TcpTrace secondaryClientEndpoint;
        public static RabbitMQBroker Broker { get; private set; }
        public static TcpTrace FederationLinkTcpProxy { get; private set; }
        public static bool RunEmbeddedBroker { get { return bool.Parse(ConfigurationManager.AppSettings["RunEmbeddedBroker"]); } }
        public static ResbitClient ResbitClient { get; private set; }

        [FixtureSetUp]
        public void TestFixtureSetup()
        {
            FederationLinkTcpProxy = new TcpTrace(@"..\..\..\..\tools\tcpTrace\tcpTrace.exe");
            StartFederationLink();

            secondaryClientEndpoint = new TcpTrace(@"..\..\..\..\tools\tcpTrace\tcpTrace.exe");
            secondaryClientEndpoint.Start(Globals.SecondaryConnectionPort, Globals.MainHostName, Globals.MainConnectionPort, "Secondary client link");

            Broker = new RabbitMQBroker(@"..\..\..\..\RabbitMQServer");
            ResbitClient = new ResbitClient(Globals.MainHostName, "guest", "guest");

            StartBroker();

            Broker.StopApp();
            Broker.Reset();
            Broker.StartAppAndWait();

            try
            {
                Assert.IsNotNull(ResbitClient.Overview(), "Broker does not appear to be running");
            }
            catch (WebException e)
            {
                Assert.Fail("Broker does not appear to be running: {0}", e);
            }

            SetupSecondaryVirtualHost();

            using (var connection = Helpers.CreateSafeShutdownConnection())
            using (var model = connection.CreateModel())
                model.ExchangeDeclare(Globals.FederationExchangeName,
                                      "x-federation",
                                      true,
                                      false,
                                      new Hashtable { { "type", "topic" }, { "upstream-set", "SecondaryUpstream" } });

            using (var connection = Helpers.CreateSafeShutdownSecondaryConnection())
            using (var model = connection.CreateModel())
                model.ExchangeDeclare(Globals.FederationExchangeName,
                                      "x-federation",
                                      true,
                                      false,
                                      new Hashtable { { "type", "topic" }, { "upstream-set", "MainUpstream" } });
        }

        private static void SetupSecondaryVirtualHost()
        {
            Broker.AddVHost("secondary");
            Broker.AddPermissions("secondary", "guest");
        }

        public static void StartFederationLink()
        {
            FederationLinkTcpProxy.Start(Globals.FederationConnectionPort, Globals.MainHostName, Globals.MainConnectionPort, "Federation");
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

            StopFederationLink();
            secondaryClientEndpoint.Stop();

            TcpTrace.StopAll(); // safety net
        }

        public static void StopFederationLink()
        {
            FederationLinkTcpProxy.Stop();
        }

        private static void StopBroker()
        {
            if (RunEmbeddedBroker)
                Broker.Stop();
        }
    }
}