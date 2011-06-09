using RabbitMQ.Client;

namespace Common
{
    public static class Helpers
    {
        public static IConnection CreateConnection()
        {
            return new ConnectionFactory
            {
                HostName = Globals.HostName,
                Port = Globals.Port,
                VirtualHost = Globals.VirtualHost
            }.CreateConnection();
        }

        public static IConnection CreateSecondaryConnection()
        {
            return new ConnectionFactory
            {
                HostName = Globals.SecondaryHostName,
                Port = Globals.SecondaryPort,
                VirtualHost = Globals.SecondaryVirtualHost
            }.CreateConnection();
        }
    }
}