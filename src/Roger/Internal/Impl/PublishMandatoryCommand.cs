using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class PublishMandatoryCommand : MandatoryCommand
    {
        public PublishMandatoryCommand(string exchange, string routingKey, byte[] body, Action<BasicReturn> basicReturnCallback, Func<RogerEndpoint, IBasicProperties> createProperties)
            : base(createProperties, basicReturnCallback, exchange, routingKey, body)
        {
        }
    }
}