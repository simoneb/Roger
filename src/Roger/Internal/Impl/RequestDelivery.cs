using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class RequestDelivery : MandatoryDelivery
    {
        public RequestDelivery(string exchange,
                               string routingKey,
                               byte[] body,
                               Func<RogerEndpoint, IBasicProperties> createProperties,
                               Action<BasicReturn> basicReturnCallback)
            : base(createProperties, basicReturnCallback, exchange, routingKey, body)
        {
        }
    }
}