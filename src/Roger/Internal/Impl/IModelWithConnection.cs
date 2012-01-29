using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal interface IModelWithConnection : IModel
    {
        IReliableConnection Connection { get; }
    }
}