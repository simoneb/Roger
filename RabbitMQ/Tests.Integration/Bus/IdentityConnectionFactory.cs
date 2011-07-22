using RabbitMQ.Client;
using Rabbus;

namespace Tests.Integration.Bus
{
    public class IdentityConnectionFactory : IConnectionFactory
    {
        private readonly IConnection connection;

        public IdentityConnectionFactory(IConnection connection)
        {
            this.connection = connection;
        }

        public IConnection CreateConnection()
        {
            return connection;
        }
    }
}