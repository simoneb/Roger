using RabbitMQ.Client;

namespace Tests.Utils
{
    public interface ICustomModel : IModel
    {
        bool Disposed { get; }
    }
}