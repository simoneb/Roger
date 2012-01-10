using System;
using RabbitMQ.Client;

namespace Roger.Internal
{
    public interface IPublishModule : IDisposable
    {
        void Initialize(IPublishingProcess publishingProcess);
        void ConnectionEstablished(IModel publishModel);
        void BeforePublish(IDeliveryCommand command, IModel publishModel, IBasicProperties properties, Action<BasicReturn> basicReturnCallback = null);
        void ConnectionUnexpectedShutdown();
    }
}