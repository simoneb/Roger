using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class PublishMandatoryCommand : MandatoryCommand
    {
        private readonly string exchange;
        private readonly string routingKey;
        private readonly byte[] body;
        private readonly Action<BasicReturn> basicReturnCallback;
        private readonly Func<RogerEndpoint, IBasicProperties> properties;

        public PublishMandatoryCommand(string exchange, string routingKey, byte[] body, Action<BasicReturn> basicReturnCallback, Func<RogerEndpoint, IBasicProperties> properties)
        {
            this.exchange = exchange;
            this.routingKey = routingKey;
            this.body = body;
            this.basicReturnCallback = basicReturnCallback;
            this.properties = properties;
        }

        public override void Execute(IModel model, RogerEndpoint endpoint, IBasicReturnHandler basicReturnHandler)
        {
            PublishMandatory(model, properties(endpoint), basicReturnCallback, routingKey, exchange, body, basicReturnHandler);
        }
    }
}