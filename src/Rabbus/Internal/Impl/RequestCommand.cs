using System;
using RabbitMQ.Client;

namespace Rabbus.Internal.Impl
{
    internal class RequestCommand : MandatoryCommand
    {
        private readonly string exchange;
        private readonly string routingKey;
        private readonly byte[] body;
        private readonly Func<RabbusEndpoint, IBasicProperties> properties;
        private readonly Action<BasicReturn> basicReturnCallback;

        public RequestCommand(string exchange, string routingKey, byte[] body, Func<RabbusEndpoint, IBasicProperties> properties, Action<BasicReturn> basicReturnCallback)
        {
            this.exchange = exchange;
            this.routingKey = routingKey;
            this.body = body;
            this.properties = properties;
            this.basicReturnCallback = basicReturnCallback;
        }

        public override void Execute(IModel model, RabbusEndpoint endpoint, IBasicReturnHandler basicReturnHandler)
        {
            PublishMandatory(model, properties(endpoint), basicReturnCallback, routingKey, exchange, body, basicReturnHandler);
        }
    }
}