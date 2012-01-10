using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class RequestCommand : MandatoryCommand
    {
        public RequestCommand(string exchange,
                              string routingKey,
                              byte[] body,
                              Func<RogerEndpoint, IBasicProperties> createProperties,
                              Action<BasicReturn> basicReturnCallback) : base(createProperties, basicReturnCallback, exchange, routingKey, body)
        {
        }
    }
}