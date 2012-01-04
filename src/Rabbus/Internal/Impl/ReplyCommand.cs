using System;
using RabbitMQ.Client;

namespace Rabbus.Internal.Impl
{
    internal class ReplyCommand : MandatoryCommand
    {
        private readonly string exchange;
        private readonly RabbusEndpoint recipient;
        private readonly byte[] body;
        private readonly Action<BasicReturn> basicReturnCallback;
        private readonly Func<RabbusEndpoint, IBasicProperties> properties;

        public ReplyCommand(string exchange, RabbusEndpoint recipient, byte[] body, Action<BasicReturn> basicReturnCallback, Func<RabbusEndpoint, IBasicProperties> properties)
        {
            this.exchange = exchange;
            this.recipient = recipient;
            this.body = body;
            this.basicReturnCallback = basicReturnCallback;
            this.properties = properties;
        }

        public override void Execute(IModel model, RabbusEndpoint endpoint, IBasicReturnHandler basicReturnHandler)
        {
            PublishMandatory(model, properties(endpoint), basicReturnCallback, recipient, exchange, body, basicReturnHandler);
        }
    }
}