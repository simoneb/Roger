using RabbitMQ.Client;

namespace Tests.Integration.Utils
{
    public interface ICustomModel : IModel
    {
        bool Disposed { get; }
    }
}