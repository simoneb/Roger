using System;
using RabbitMQ.Client;

namespace Roger.Internal
{
    internal abstract class AbstractDelivery : IDelivery
    {
        private readonly Func<RogerEndpoint, IBasicProperties> createProperties;
        private readonly Action<BasicReturn> basicReturnCallback;

        protected AbstractDelivery(Func<RogerEndpoint, IBasicProperties> createProperties, Action<BasicReturn> basicReturnCallback = null)
        {
            this.createProperties = createProperties;
            this.basicReturnCallback = basicReturnCallback;
        }

        public void Execute(IModel model, RogerEndpoint endpoint, IPublishModule modules)
        {
            var properties = createProperties(endpoint);

            modules.BeforePublish(this, model, properties, basicReturnCallback);

            ExecuteCore(model, properties);
        }

        protected abstract void ExecuteCore(IModel model, IBasicProperties properties);
    }
}