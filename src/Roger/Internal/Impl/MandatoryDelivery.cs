using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal abstract class MandatoryDelivery : AbstractDelivery
    {
        private readonly string exchange;
        private readonly string routingKey;
        private readonly byte[] body;

        protected MandatoryDelivery(Func<RogerEndpoint, IBasicProperties> createProperties,
                                    Action<BasicReturn> basicReturnCallback,
                                    string exchange,
                                    string routingKey,
                                    byte[] body)
            : base(createProperties, basicReturnCallback)
        {
            this.exchange = exchange;
            this.routingKey = routingKey;
            this.body = body;
        }

        protected sealed override void ExecuteInternal(IModel model, IBasicProperties properties)
        {
            model.BasicPublish(exchange, routingKey, true, false, properties, body);
        }
    }
}