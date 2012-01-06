using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class PublishCommandFactory : AbstractDeliveryCommandFactory
    {
        private readonly string exchange;
        private readonly string routingKey;
        private readonly byte[] body;

        public PublishCommandFactory(Type messageType, string exchange, string routingKey, byte[] body) : base(messageType)
        {
            this.exchange = exchange;
            this.routingKey = routingKey;
            this.body = body;
        }

        public override IDeliveryCommand Create(IModel model, IIdGenerator idGenerator, ITypeResolver typeResolver, IMessageSerializer serializer, ISequenceGenerator sequenceGenerator)
        {
            var properties = CreateProperties(model, idGenerator, typeResolver, serializer, sequenceGenerator);

            return new PublishCommand(exchange, routingKey, body, properties);
        }
    }
}