using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class PublishCommand : AbstractDeliveryCommand
    {
        private readonly string exchange;
        private readonly string routingKey;
        private readonly byte[] body;

        public PublishCommand(string exchange, string routingKey, byte[] body, Func<RogerEndpoint, IBasicProperties> createProperties) : base(createProperties)
        {
            this.exchange = exchange;
            this.routingKey = routingKey;
            this.body = body;
        }

        protected override void ExecuteInternal(IModel model, IBasicProperties properties)
        {
            model.BasicPublish(exchange, routingKey, properties, body);
        }
    }
}