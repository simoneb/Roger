using System;
using System.Collections.Specialized;
using Common.Logging;
using Common.Logging.Simple;
using RabbitMQ.Client;

namespace Common
{
    public static class Helpers
    {
        public static IConnection CreateConnection()
        {
            return new ConnectionFactory
            {
                HostName = Constants.HostName,
                Port = Constants.MainPort,
                VirtualHost = Constants.MainVirtualHost
            }.CreateConnection();
        }

        public static IConnection CreateConnectionToSecondaryVirtualHostOnAlternativePort()
        {
            return new ConnectionFactory
            {
                HostName = Constants.HostName,
                Port = Constants.AlternativeConnectionPort,
                VirtualHost = Constants.SecondaryVirtualHost
            }.CreateConnection();
        }

        public static IConnection CreateConnectionOnAlternativePort()
        {
            return new ConnectionFactory
            {
                HostName = Constants.HostName,
                Port = Constants.AlternativeConnectionPort,
                VirtualHost = Constants.MainVirtualHost
            }.CreateConnection();
        }

        public static IConnection CreateSafeShutdownConnection()
        {
            return new SafeShutdownConnection(CreateConnection());
        }

        public static IConnection CreateSafeShutdownConnectionToSecondaryVirtualHostOnAlternativePort()
        {
            return new SafeShutdownConnection(CreateConnectionToSecondaryVirtualHostOnAlternativePort());
        }

        public static void InitializeTestLogging()
        {
            var props = new NameValueCollection { { "showLogName", "true" }, { "showDateTime", "false" } };

            LogManager.Adapter = RunningOnTeamCity
                                     ? (ILoggerFactoryAdapter)new ConsoleOutLoggerFactoryAdapter(props)
                                     : new TraceLoggerFactoryAdapter(props);
        }

        private static bool RunningOnTeamCity
        {
            get { return Environment.GetEnvironmentVariable("TEAMCITY_VERSION") != null; }
        }
    }
}