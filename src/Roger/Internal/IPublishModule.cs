using System;
using RabbitMQ.Client;

namespace Roger.Internal
{
    internal interface IPublishModule : IDisposable
    {
        void Initialize(IPublishingProcess publishingProcess);
        void BeforePublishEnabled(IModel publishModel);
        void BeforePublish(IDelivery delivery, IModel publishModel, IBasicProperties properties, Action<BasicReturn> basicReturnCallback);
        void AfterPublishDisabled(IModel publishModel);
    }
}