using RabbitMQ.Client;

namespace Roger.Internal
{
    public interface IQueueFactory
    {
        QueueDeclareOk Create(IModel model);
    }
}