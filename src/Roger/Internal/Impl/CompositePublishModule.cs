using System;
using System.Threading;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class CompositePublishModule : IPublishModule
    {
        private readonly IPublishModule[] inner;
        private int disposed;

        public CompositePublishModule(params IPublishModule[] inner)
        {
            this.inner = inner;
        }

        public void Initialize(IPublishingProcess publishingProcess)
        {
            foreach (var module in inner)
                module.Initialize(publishingProcess);
        }

        public void BeforePublishEnabled(IModel publishModel)
        {
            foreach (var module in inner)
                module.BeforePublishEnabled(publishModel);
        }

        public void BeforePublish(IDeliveryCommand command, IModel publishModel, IBasicProperties properties, Action<BasicReturn> basicReturnCallback = null)
        {
            foreach (var module in inner)
                module.BeforePublish(command, publishModel, properties, basicReturnCallback);
        }

        public void AfterPublishDisabled()
        {
            foreach (var module in inner)
                module.AfterPublishDisabled();
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) == 1)
                return;

            foreach (var module in inner)
                module.Dispose();
        }
    }
}