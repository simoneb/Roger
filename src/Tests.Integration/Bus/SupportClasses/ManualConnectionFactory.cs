using System;
using RabbitMQ.Client;
using Roger;

namespace Tests.Integration.Bus.SupportClasses
{
    public class ManualConnectionFactory : IConnectionFactory
    {
        private readonly Func<IConnection> connection;

        public ManualConnectionFactory(Func<IConnection> connection)
        {
            this.connection = connection;
        }

        public IConnection CreateConnection()
        {
            return connection();
        }
    }
}