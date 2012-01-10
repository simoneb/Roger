using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class PublishMandatoryCommand : MandatoryCommand
    {
        private readonly string exchange;
        private readonly string routingKey;
        private readonly byte[] body;

        public PublishMandatoryCommand(string exchange,
                                       string routingKey,
                                       byte[] body,
                                       Action<BasicReturn> basicReturnCallback,
                                       Func<RogerEndpoint, IBasicProperties> createProperties)
            : base(createProperties, basicReturnCallback, exchange, routingKey, body)
        {
            this.exchange = exchange;
            this.routingKey = routingKey;
            this.body = body;
        }
    }
}