using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class ReplyCommandFactory : AbstractDeliveryCommandFactory
    {
        private readonly string exchange;
        private readonly CurrentMessageInformation currentMessage;
        private readonly byte[] body;
        private readonly Action<BasicReturn> basicReturnCallback;

        public ReplyCommandFactory(Type messageType, string exchange, CurrentMessageInformation currentMessage, byte[] body, Action<BasicReturn> basicReturnCallback) : base(messageType)
        {
            this.exchange = exchange;
            this.currentMessage = currentMessage;
            this.body = body;
            this.basicReturnCallback = basicReturnCallback;
        }

        public override IDeliveryCommand Create(IModel model, IIdGenerator idGenerator, ITypeResolver typeResolver, IMessageSerializer serializer, ISequenceGenerator sequenceGenerator)
        {
            var properties = CreateProperties(model, idGenerator, typeResolver, serializer, sequenceGenerator, p => p.CorrelationId = currentMessage.CorrelationId);

            return new ReplyCommand(exchange, currentMessage.Endpoint, body, basicReturnCallback, properties);
        }
    }
}