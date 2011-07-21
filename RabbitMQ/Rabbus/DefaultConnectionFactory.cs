using RabbitMQ.Client;

namespace Rabbus
{
    public class DefaultConnectionFactory : IConnectionFactory
    {
        private readonly ConnectionFactory m_inner;

        public DefaultConnectionFactory(string hostName,
                                        string virtualHost = ConnectionFactory.DefaultVHost,
                                        int port = 5672,
                                        string userName = ConnectionFactory.DefaultUser,
                                        string password =  ConnectionFactory.DefaultPass)
        {
            m_inner = new ConnectionFactory
            {
                HostName = hostName,
                VirtualHost = virtualHost,
                Port = port,
                UserName = userName,
                Password = password
            };
        }

        public IConnection CreateConnection()
        {
            return m_inner.CreateConnection();
        }
    }
}