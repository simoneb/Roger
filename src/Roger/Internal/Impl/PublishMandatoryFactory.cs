using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class PublishMandatoryFactory : AbstractDeliveryFactory
    {
        private readonly string exchange;
        private readonly string routingKey;
        private readonly byte[] body;
        private readonly Action<BasicReturn> basicReturnCallback;

        public PublishMandatoryFactory(Type messageType,
                                               string exchange,
                                               string routingKey,
                                               byte[] body,
                                               Action<BasicReturn> basicReturnCallback,
                                               bool persistent,
                                               bool sequence) : base(messageType, persistent, sequence)
        {
            this.exchange = exchange;
            this.routingKey = routingKey;
            this.body = body;
            this.basicReturnCallback = basicReturnCallback;
        }

        protected override IDelivery CreateCore(Func<RogerEndpoint, IBasicProperties> createProperties)
        {
            return new PublishMandatoryDelivery(exchange, routingKey, body, basicReturnCallback, createProperties);
        }
    }
}