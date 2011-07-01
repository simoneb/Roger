using RabbitMQ.Client;

namespace Shoveling.Test.Utils
{
    public interface ICustomModel : IModel
    {
        bool Disposed { get; }
    }
}