using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class PublishFactory : AbstractDeliveryFactory
    {
        private readonly string exchange;
        private readonly string routingKey;
        private readonly byte[] body;
        private readonly bool persistent;

        public PublishFactory(Type messageType, string exchange, string routingKey, byte[] body, bool persistent) : base(messageType, persistent)
        {
            this.exchange = exchange;
            this.routingKey = routingKey;
            this.body = body;
            this.persistent = persistent;
        }

        protected override IDelivery CreateCore(Func<RogerEndpoint, IBasicProperties> createProperties)
        {
            return new PublishDelivery(exchange, routingKey, body, createProperties);
        }
    }
}