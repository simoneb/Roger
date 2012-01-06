using RabbitMQ.Client;

namespace Roger
{
    public interface IConnectionFactory
    {
        IConnection CreateConnection();
    }
}