using System;
using System.Collections.Generic;
using System.Threading;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class PublishModuleCollection : IPublishModule
    {
        private readonly LinkedList<IPublishModule> inner;
        private int disposed;

        public PublishModuleCollection(params IPublishModule[] inner)
        {
            this.inner = new LinkedList<IPublishModule>(inner);
        }

        public void AddFirst(IPublishModule module)
        {
            inner.AddFirst(module);
        }

        public void Add(params IPublishModule[] modules)
        {
            foreach (var module in modules)
                inner.AddLast(module);
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

        public void BeforePublish(IDelivery delivery, IModel publishModel, IBasicProperties properties, Action<BasicReturn> basicReturnCallback)
        {
            foreach (var module in inner)
                module.BeforePublish(delivery, publishModel, properties, basicReturnCallback);
        }

        public void AfterPublishDisabled(IModel publishModel)
        {
            foreach (var module in inner)
                module.AfterPublishDisabled(publishModel);
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