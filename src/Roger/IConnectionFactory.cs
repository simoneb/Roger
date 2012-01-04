using RabbitMQ.Client;

namespace Rabbus
{
    public interface IConnectionFactory
    {
        IConnection CreateConnection();
    }
}