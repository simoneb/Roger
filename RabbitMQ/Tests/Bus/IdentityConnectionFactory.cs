using RabbitMQ.Client;
using Rabbus;

namespace Tests.Bus
{
    public class IdentityConnectionFactory : IConnectionFactory
    {
        private readonly IConnection m_connection;

        public IdentityConnectionFactory(IConnection connection)
        {
            m_connection = connection;
        }

        public IConnection CreateConnection()
        {
            return m_connection;
        }
    }
}