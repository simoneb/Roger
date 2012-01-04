using System;
using RabbitMQ.Client;

namespace Rabbus.Internal.Impl
{
    internal class PublishCommand : IDeliveryCommand
    {
        private readonly string exchange;
        private readonly string routingKey;
        private readonly byte[] body;
        private readonly Func<RabbusEndpoint, IBasicProperties> properties;

        public PublishCommand(string exchange, string routingKey, byte[] body, Func<RabbusEndpoint, IBasicProperties> properties)
        {
            this.exchange = exchange;
            this.routingKey = routingKey;
            this.body = body;
            this.properties = properties;
        }

        public void Execute(IModel model, RabbusEndpoint endpoint, IBasicReturnHandler basicReturnHandler)
        {
            model.BasicPublish(exchange, routingKey, properties(endpoint), body);
        }
    }
}