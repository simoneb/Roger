using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class SendCommandFactory : AbstractDeliveryCommandFactory
    {
        private readonly string exchange;
        private readonly RogerEndpoint recipient;
        private readonly byte[] body;
        private readonly Action<BasicReturn> basicReturnCallback;
        private readonly bool persistent;

        public SendCommandFactory(Type messageType, string exchange, RogerEndpoint recipient, byte[] body, Action<BasicReturn> basicReturnCallback, bool persistent) : base(messageType)
        {
            this.exchange = exchange;
            this.recipient = recipient;
            this.body = body;
            this.basicReturnCallback = basicReturnCallback;
            this.persistent = persistent;
        }

        public override IDeliveryCommand Create(IModel model, IIdGenerator idGenerator, ITypeResolver typeResolver, IMessageSerializer serializer, ISequenceGenerator sequenceGenerator)
        {
            var properties = CreatePropertiesFactory(model, idGenerator, typeResolver, serializer, sequenceGenerator, persistent);

            return new SendCommand(exchange, recipient, body, basicReturnCallback, properties);
        }
    }
}