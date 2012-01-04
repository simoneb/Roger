using System;
using RabbitMQ.Client;

namespace Rabbus.Internal.Impl
{
    internal class PublishMandatoryCommand : MandatoryCommand
    {
        private readonly string exchange;
        private readonly string routingKey;
        private readonly byte[] body;
        private readonly Action<BasicReturn> basicReturnCallback;
        private readonly Func<RabbusEndpoint, IBasicProperties> properties;

        public PublishMandatoryCommand(string exchange, string routingKey, byte[] body, Action<BasicReturn> basicReturnCallback, Func<RabbusEndpoint, IBasicProperties> properties)
        {
            this.exchange = exchange;
            this.routingKey = routingKey;
            this.body = body;
            this.basicReturnCallback = basicReturnCallback;
            this.properties = properties;
        }

        public override void Execute(IModel model, RabbusEndpoint endpoint, IBasicReturnHandler basicReturnHandler)
        {
            PublishMandatory(model, properties(endpoint), basicReturnCallback, routingKey, exchange, body, basicReturnHandler);
        }
    }
}