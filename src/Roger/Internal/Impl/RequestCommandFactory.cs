using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class RequestCommandFactory : AbstractDeliveryCommandFactory
    {
        private readonly string exchange;
        private readonly string routingKey;
        private readonly byte[] body;
        private readonly Action<BasicReturn> basicReturnCallback;

        public RequestCommandFactory(Type messageType, string exchange, string routingKey, byte[] body, Action<BasicReturn> basicReturnCallback) : base(messageType)
        {
            this.exchange = exchange;
            this.routingKey = routingKey;
            this.body = body;
            this.basicReturnCallback = basicReturnCallback;
        }

        public override IDeliveryCommand Create(IModel model, IIdGenerator idGenerator, ITypeResolver typeResolver, IMessageSerializer serializer, ISequenceGenerator sequenceGenerator)
        {
            var properties = CreateProperties(model, idGenerator, typeResolver, serializer, sequenceGenerator, p => p.CorrelationId = idGenerator.Next());

            return new RequestCommand(exchange, routingKey, body, properties, basicReturnCallback);
        }
    }
}