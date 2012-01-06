using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class NoLocalFilter : IMessageFilter
    {
        private readonly Func<RogerEndpoint> endpoint;

        public NoLocalFilter(Func<RogerEndpoint> endpoint)
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