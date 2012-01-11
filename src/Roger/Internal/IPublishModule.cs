using System;
using RabbitMQ.Client;

namespace Roger.Internal
{
    public interface IPublishModule : IDisposable
    {
        void Initialize(IPublishingProcess publishingProcess);
        void BeforePublishEnabled(IModel publishModel);
        void BeforePublish(IDeliveryCommand command, IModel publishModel, IBasicProperties properties, Action<BasicReturn> basicReturnCallback = null);
        void AfterPublishDisabled(IModel publishModel);
    }
}