using System;
using RabbitMQ.Client;

namespace Roger.Internal
{
    public interface IPublishModule : IDisposable
    {
        void Initialize(IPublishingProcess publishingProcess);
        void BeforePublishEnabled(IModel publishModel);
        void BeforePublish(IDelivery delivery, IModel publishModel, IBasicProperties properties, Action<BasicReturn> basicReturnCallback);
        void AfterPublishDisabled(IModel publishModel);
    }
}