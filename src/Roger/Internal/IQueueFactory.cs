using RabbitMQ.Client;

namespace Roger.Internal
{
    internal interface IQueueFactory
    {
        QueueDeclareOk Create(IModel model);
    }
}