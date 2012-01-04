using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Rabbus.Internal.Impl
{
    internal class NoLocalFilter : IMessageFilter
    {
        private readonly Func<RabbusEndpoint> endpoint;

        public NoLocalFilter(Func<RabbusEndpoint> endpoint)
        {
            this.endpoint = endpoint;
        }

        public IEnumerable<CurrentMessageInformation> Filter(IEnumerable<CurrentMessageInformation> input, IModel model)
        {
            foreach (var message in input)
            {
                if(message.Endpoint.Equals(endpoint()))
                    model.BasicAck(message.DeliveryTag, false);
                else
                    yield return message;
            }
        }
    }
}