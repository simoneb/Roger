using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class SendFactory : AbstractDeliveryFactory
    {
        private readonly string exchange;
        private readonly RogerEndpoint recipient;
        private readonly byte[] body;
        private readonly Action<BasicReturn> basicReturnCallback;

        public SendFactory(Type messageType, string exchange, RogerEndpoint recipient, byte[] body, Action<BasicReturn> basicReturnCallback, bool persistent) : base(messageType, persistent)
        {
            this.exchange = exchange;
            this.recipient = recipient;
            this.body = body;
            this.basicReturnCallback = basicReturnCallback;
        }

        protected override IDelivery CreateCore(Func<RogerEndpoint, IBasicProperties> createProperties)
        {
            return new SendDelivery(exchange, recipient, body, basicReturnCallback, createProperties);
        }
    }
}