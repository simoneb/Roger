using RabbitMQ.Client;

namespace Roger.Internal
{
    public interface IPublishModule
    {
        void ConnectionEstablished(IModel publishModel);
        void BeforePublish(IDeliveryCommand command, IModel publishModel);
        void Initialize(IPublishingProcess publishingProcess);
    }
}