using System;
using RabbitMQ.Client;

namespace Rabbus
{
    public class DefaultConnectionFactory : IConnectionFactory
    {
        private readonly ConnectionFactory inner;

        /// <summary>
        /// Creates a new instance of <see cref="DefaultConnectionFactory"/>
        /// </summary>
        /// <param name="uri">A <see cref="Uri"/> in the form <example>amqp://user:pass@host:10000/vhost</example> as specified in <c>http://www.rabbitmq.com/uri-spec.html</c></param>
        public DefaultConnectionFactory(Uri uri)
        {
            inner = new ConnectionFactory {uri = uri};
        }

        public DefaultConnectionFactory(string hostName,
                                        string virtualHost = ConnectionFactory.DefaultVHost,
                                        int port = 5672,
                                        string userName = ConnectionFactory.DefaultUser,
                                        string password =  ConnectionFactory.DefaultPass)
        {
            inner = new ConnectionFactory
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
            return inner.CreateConnection();
        }
    }
}