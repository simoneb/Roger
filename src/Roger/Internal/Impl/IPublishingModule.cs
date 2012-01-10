using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal interface IPublishingModule
    {
        void ConnectionEstablished(IModel publishModel);
        void BeforePublish(IDeliveryCommand command, IModel publishModel);
        void Initialize(IPublishingProcess publishingProcess);
    }
}