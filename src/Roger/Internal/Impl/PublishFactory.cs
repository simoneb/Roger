using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class PublishFactory : AbstractDeliveryFactory
    {
        private readonly string exchange;
        private readonly string routingKey;
        private readonly byte[] body;

        public PublishFactory(Type messageType, string exchange, string routingKey, byte[] body, bool persistent, bool sequence) : base(messageType, persistent, sequence)
        {
            this.exchange = exchange;
            this.routingKey = routingKey;
            this.body = body;
        }

        protected override IDelivery CreateCore(Func<RogerEndpoint, IBasicProperties> createProperties)
        {
            return new PublishDelivery(exchange, routingKey, body, createProperties);
        }
    }
}