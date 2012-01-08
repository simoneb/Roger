using System;
using System.Collections;
using System.Configuration;
using System.Net;
using Common;
using MbUnit.Framework;
using Resbit;
using Spring.Messaging.Amqp.Rabbit.Admin;

namespace Tests.Integration.Utils
{
    [AssemblyFixture]
    public class Bootstrap
    {
        private TcpTrace secondaryClientEndpoint;
        private const string All = ".*";
        public static RabbitBrokerAdmin Broker { get; private set; }
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

            Environment.SetEnvironmentVariable("RABBITMQ_SERVER", @"..\..\..\..\RabbitMQServer");
            Broker = new RabbitBrokerAdmin("rabbit@LOCALHOST"){StartupTimeout = 10000};
            ResbitClient = new ResbitClient(Globals.MainHostName, "guest", "guest");

            StartBroker();

            Broker.StopBrokerApplication();
            Broker.ResetNode();
            Broker.StartBrokerApplication();

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
            Broker.AddVhost(Globals.SecondaryVirtualHost);
            Broker.SetPermissions("guest", All, All, All, Globals.SecondaryVirtualHost);
        }

        public static void StartFederationLink()
        {
            FederationLinkTcpProxy.Start(Globals.FederationConnectionPort, Globals.MainHostName, Globals.MainConnectionPort, "Federation");
        }

        private static void StartBroker()
        {
            if (RunEmbeddedBroker)
                Broker.StartBrokerApplication();
        }

        [FixtureTearDown]
        public void TestFixtureTeardown()
        {
            Broker.StopBrokerApplication();
            Broker.ResetNode();
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
                Broker.StopNode();
        }
    }
}