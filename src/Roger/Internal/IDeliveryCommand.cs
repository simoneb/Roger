using RabbitMQ.Client;

namespace Roger.Internal
{
    public interface IDeliveryCommand
    {
        void Execute(IModel model, RogerEndpoint endpoint, IPublishModule modules);
    }
}