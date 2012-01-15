using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class RequestFactory : AbstractDeliveryFactory
    {
        private readonly string exchange;
        private readonly string routingKey;
        private readonly byte[] body;
        private readonly bool persistent;
        private readonly Action<BasicReturn> basicReturnCallback;

        public RequestFactory(Type messageType, string exchange, string routingKey, byte[] body, bool persistent, Action<BasicReturn> basicReturnCallback) : base(messageType)
        {
            this.exchange = exchange;
            this.routingKey = routingKey;
            this.body = body;
            this.persistent = persistent;
            this.basicReturnCallback = basicReturnCallback;
        }

        public override IDelivery Create(IModel model, IIdGenerator idGenerator, ITypeResolver typeResolver, IMessageSerializer serializer, ISequenceGenerator sequenceGenerator)
        {
            var properties = CreatePropertiesFactory(model, idGenerator, typeResolver, serializer, sequenceGenerator, persistent, p => p.CorrelationId = idGenerator.Next());

            return new RequestDelivery(exchange, routingKey, body, properties, basicReturnCallback);
        }
    }
}