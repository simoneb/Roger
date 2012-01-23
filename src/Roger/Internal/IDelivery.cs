using RabbitMQ.Client;

namespace Roger.Internal
{
    internal interface IDelivery
    {
        void Execute(IModel model, RogerEndpoint endpoint, IPublishModule module);
    }
}