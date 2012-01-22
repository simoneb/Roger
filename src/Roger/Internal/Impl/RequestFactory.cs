using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class RequestFactory : AbstractDeliveryFactory
    {
        private readonly string exchange;
        private readonly string routingKey;
        private readonly byte[] body;
        private readonly Action<BasicReturn> basicReturnCallback;

        public RequestFactory(Type messageType,
                              string exchange,
                              string routingKey,
                              byte[] body,
                              Action<BasicReturn> basicReturnCallback,
                              bool persistent) : base(messageType, persistent)
        {
            this.exchange = exchange;
            this.routingKey = routingKey;
            this.body = body;
            this.basicReturnCallback = basicReturnCallback;
        }

        protected override IDelivery CreateCore(Func<RogerEndpoint, IBasicProperties> createProperties)
        {
            return new RequestDelivery(exchange, routingKey, body, createProperties, basicReturnCallback);
        }
    }
}