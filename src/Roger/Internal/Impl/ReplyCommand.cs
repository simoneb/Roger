using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class ReplyCommand : MandatoryCommand
    {
        private readonly string exchange;
        private readonly RogerEndpoint recipient;
        private readonly byte[] body;
        private readonly Action<BasicReturn> basicReturnCallback;
        private readonly Func<RogerEndpoint, IBasicProperties> properties;

        public ReplyCommand(string exchange, RogerEndpoint recipient, byte[] body, Action<BasicReturn> basicReturnCallback, Func<RogerEndpoint, IBasicProperties> properties)
        {
            this.exchange = exchange;
            this.recipient = recipient;
            this.body = body;
            this.basicReturnCallback = basicReturnCallback;
            this.properties = properties;
        }

        public override void Execute(IModel model, RogerEndpoint endpoint, IBasicReturnHandler basicReturnHandler)
        {
            PublishMandatory(model, properties(endpoint), basicReturnCallback, recipient, exchange, body, basicReturnHandler);
        }
    }
}