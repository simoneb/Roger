using System;
using RabbitMQ.Client;
using Rabbus;

namespace Tests.Integration.Bus.SupportClasses
{
    public class IdentityConnectionFactory : IConnectionFactory
    {
        private readonly Func<IConnection> connection;

        public IdentityConnectionFactory(Func<IConnection> connection)
        {
            this.connection = connection;
        }

        public IConnection CreateConnection()
        {
            return connection();
        }
    }
}