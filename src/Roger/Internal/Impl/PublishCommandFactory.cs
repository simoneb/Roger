using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class PublishCommandFactory : AbstractDeliveryCommandFactory
    {
        private readonly string exchange;
        private readonly string routingKey;
        private readonly byte[] body;
        private readonly bool persistent;

        public PublishCommandFactory(Type messageType, string exchange, string routingKey, byte[] body, bool persistent) : base(messageType)
        {
            this.exchange = exchange;
            this.routingKey = routingKey;
            this.body = body;
            this.persistent = persistent;
        }

        public override IDeliveryCommand Create(IModel model, IIdGenerator idGenerator, ITypeResolver typeResolver, IMessageSerializer serializer, ISequenceGenerator sequenceGenerator)
        {
            var createProperties = CreatePropertiesFactory(model, idGenerator, typeResolver, serializer, sequenceGenerator, persistent);

            return new PublishCommand(exchange, routingKey, body, createProperties);
        }
    }
}