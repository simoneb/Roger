using System;
using RabbitMQ.Client;

namespace Rabbus.Internal.Impl
{
    internal class SendCommandFactory : AbstractDeliveryCommandFactory
    {
        private readonly string exchange;
        private readonly RabbusEndpoint recipient;
        private readonly byte[] body;
        private readonly Action<BasicReturn> basicReturnCallback;

        public SendCommandFactory(Type messageType, string exchange, RabbusEndpoint recipient, byte[] body, Action<BasicReturn> basicReturnCallback) : base(messageType)
        {
            this.exchange = exchange;
            this.recipient = recipient;
            this.body = body;
            this.basicReturnCallback = basicReturnCallback;
        }

        public override IDeliveryCommand Create(IModel model, IIdGenerator idGenerator, ITypeResolver typeResolver, IMessageSerializer serializer, ISequenceGenerator sequenceGenerator)
        {
            var properties = CreateProperties(model, idGenerator, typeResolver, serializer, sequenceGenerator);

            return new SendCommand(exchange, recipient, body, basicReturnCallback, properties);
        }
    }
}