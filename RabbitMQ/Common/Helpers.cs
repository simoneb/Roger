using RabbitMQ.Client;

namespace Common
{
    public static class Helpers
    {
        public static IConnection CreateConnection()
        {
            return new ConnectionFactory {HostName = Globals.HostName, Port = Globals.Port}.CreateConnection();
        }
    }
}