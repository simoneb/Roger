using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class ReplyFactory : AbstractDeliveryFactory
    {
        private readonly string exchange;
        private readonly CurrentMessageInformation currentMessage;
        private readonly byte[] body;
        private readonly Action<BasicReturn> basicReturnCallback;

        public ReplyFactory(Type messageType,
                            string exchange,
                            CurrentMessageInformation currentMessage,
                            byte[] body,
                            Action<BasicReturn> basicReturnCallback,
                            bool persistent,
                            bool sequence) : base(messageType, persistent, sequence)
        {
            this.exchange = exchange;
            this.currentMessage = currentMessage;
            this.body = body;
            this.basicReturnCallback = basicReturnCallback;
        }

        protected override IDelivery CreateCore(Func<RogerEndpoint, IBasicProperties> createProperties)
        {
            return new ReplyDelivery(exchange, currentMessage.Endpoint, body, basicReturnCallback, createProperties);
        }

        protected override void FillAdditionalProperties(IBasicProperties properties)
        {
            properties.CorrelationId = currentMessage.MessageId;
        }
    }
}