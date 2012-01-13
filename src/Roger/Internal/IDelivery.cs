using RabbitMQ.Client;

namespace Roger.Internal
{
    /// <summary>
    /// Represents a message delivery
    /// </summary>
    public interface IDelivery
    {
        void Execute(IModel model, RogerEndpoint endpoint, IPublishModule modules);
    }
}