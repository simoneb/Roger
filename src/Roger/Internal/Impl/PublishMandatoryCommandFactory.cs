using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class PublishMandatoryCommandFactory : AbstractDeliveryCommandFactory
    {
        private readonly string exchange;
        private readonly string routingKey;
        private readonly byte[] body;
        private readonly Action<BasicReturn> basicReturnCallback;
        private readonly bool persistent;

        public PublishMandatoryCommandFactory(Type messageType, string exchange, string routingKey, byte[] body, Action<BasicReturn> basicReturnCallback, bool persistent) : base(messageType)
        {
            this.exchange = exchange;
            this.routingKey = routingKey;
            this.body = body;
            this.basicReturnCallback = basicReturnCallback;
            this.persistent = persistent;
        }

        public override IDeliveryCommand Create(IModel model, IIdGenerator idGenerator, ITypeResolver typeResolver, IMessageSerializer serializer, ISequenceGenerator sequenceGenerator)
        {
            var createProperties = CreatePropertiesFactory(model, idGenerator, typeResolver, serializer, sequenceGenerator, persistent);

            return new PublishMandatoryCommand(exchange, routingKey, body, basicReturnCallback, createProperties);
        }
    }
}