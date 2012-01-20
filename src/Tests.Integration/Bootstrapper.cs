using System;
using System.Collections;
using System.Configuration;
using System.Net;
using Common;
using MbUnit.Framework;
using Resbit;
using Spring.Erlang;
using Spring.Messaging.Amqp.Rabbit.Admin;
using Tests.Integration.Utils;

namespace Tests.Integration
{
    [AssemblyFixture]
    public static class Bootstrapper
    {
        private static readonly TcpTrace AlternativePortProxy = new TcpTrace(@"..\..\..\..\tools\tcpTrace\tcpTrace.exe");
        private static readonly TcpTrace FederationProxy = new TcpTrace(@"..\..\..\..\tools\tcpTrace\tcpTrace.exe");
        private static bool RunEmbeddedBroker { get { return bool.Parse(ConfigurationManager.AppSettings["RunEmbeddedBroker"]); } }
        private static bool ResetBrokerAtStartup { get { return bool.Parse(ConfigurationManager.AppSettings["ResetBrokerAtStartup"]); } }
        public static RabbitBrokerAdmin Broker { get; private set; }
        public static ResbitClient BrokerHttp { get; private set; }

        static Bootstrapper()
        {
            Helpers.InitializeTestLogging();
        }
       
        [FixtureSetUp]
        public static void GlobalFixtureSetup()
        {
            StartFederationProxy();
            StartAlternativePortProxy();

            Environment.SetEnvironmentVariable("RABBITMQ_SERVER", @"..\..\..\..\RabbitMQServer");
            Environment.SetEnvironmentVariable("RABBITMQ_SERVER_START_ARGS", "-setcookie RogerIntegrationTests");

            Broker = new RabbitBrokerAdmin(cookie: "RogerIntegrationTests") {StartupTimeout = 10000};
            BrokerHttp = new ResbitClient(Constants.HostName);

            TryStartBroker();

            EnsureBrokerRunning();

            SetupSecondaryVirtualHost();

            SetupFederationExchanges();
        }

        private static void TryStartBroker()
        {
            if (RunEmbeddedBroker)
                Broker.StartBrokerApplication();

            if(ResetBrokerAtStartup)
            {
                Broker.StopBrokerApplication();
                Broker.ResetNode();
                Broker.StartBrokerApplication();
            }
        }

        private static void SetupFederationExchanges()
        {
            using (var connection = Helpers.CreateSafeShutdownConnection())
            using (var model = connection.CreateModel())
                model.ExchangeDeclare(Constants.FederationExchangeName,
                                      "x-federation",
                                      true,
                                      false,
                                      new Hashtable {{"type", "topic"}, {"upstream-set", "SecondaryUpstream"}});

            using (var connection = Helpers.CreateSafeShutdownConnectionToSecondaryVirtualHostOnAlternativePort())
            using (var model = connection.CreateModel())
                model.ExchangeDeclare(Constants.FederationExchangeName,
                                      "x-federation",
                                      true,
                                      false,
                                      new Hashtable {{"type", "topic"}, {"upstream-set", "MainUpstream"}});
        }

        private static void EnsureBrokerRunning()
        {
            try
            {
                Assert.IsNotNull(BrokerHttp.Overview(), "Broker does not appear to be running");
            }
            catch (WebException e)
            {
                Assert.Fail("Broker does not appear to be running: {0}", e);
            }
        }

        private static void SetupSecondaryVirtualHost()
        {
            try
            {
                Broker.AddVhost(Constants.SecondaryVirtualHost);
            }
            catch (ErlangErrorRpcException)
            {
                // probably vhost exists already
            }
            Broker.SetPermissions("guest", ".*", ".*", ".*", Constants.SecondaryVirtualHost);
        }

        public static void StartFederationProxy()
        {
            FederationProxy.Start(Constants.FederationConnectionPort, Constants.HostName, Constants.MainPort, "Federation");
        }

        public static void StopFederationProxy()
        {
            FederationProxy.Stop();
        }

        public static void StartAlternativePortProxy()
        {
            AlternativePortProxy.Start(Constants.AlternativeConnectionPort, 
                                       Constants.HostName, 
                                       Constants.MainPort, 
                                       string.Format("{0} > {1}:{2}", Constants.AlternativeConnectionPort,
                                                     Constants.HostName,
                                                     Constants.MainPort));
        }

        public static void StopAlternativePortProxy()
        {
            AlternativePortProxy.Stop();
        }

        [FixtureTearDown]
        public static void TestFixtureTeardown()
        {
            TryStopBroker();

            StopFederationProxy();
            StopAlternativePortProxy();

            TcpTrace.StopAll(); // safety net
        }

        private static void TryStopBroker()
        {
            if (RunEmbeddedBroker)
                Broker.StopNode();
        }
    }
}