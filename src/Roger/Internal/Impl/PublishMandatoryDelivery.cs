using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class PublishMandatoryDelivery : MandatoryDelivery
    {
        public PublishMandatoryDelivery(string exchange,
                                        string routingKey,
                                        byte[] body,
                                        Action<BasicReturn> basicReturnCallback,
                                        Func<RogerEndpoint, IBasicProperties> createProperties)
            : base(createProperties, basicReturnCallback, exchange, routingKey, body)
        {
        }
    }
}