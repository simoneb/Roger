using RabbitMQ.Client;

namespace Shoveling.Test
{
    public interface ICustomModel : IModel
    {
        bool Disposed { get; }
    }
}