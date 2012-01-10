using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class ReplyCommand : MandatoryCommand
    {
        public ReplyCommand(string exchange,
                            RogerEndpoint recipient,
                            byte[] body,
                            Action<BasicReturn> basicReturnCallback,
                            Func<RogerEndpoint, IBasicProperties> createProperties)
            : base(createProperties, basicReturnCallback, exchange, recipient, body)
        {
        }
    }
}