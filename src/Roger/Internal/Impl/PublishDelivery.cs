using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class PublishDelivery : AbstractDelivery
    {
        private readonly string exchange;
        private readonly string routingKey;
        private readonly byte[] body;

        public PublishDelivery(string exchange, string routingKey, byte[] body, Func<RogerEndpoint, IBasicProperties> createProperties) : base(createProperties)
        {
            this.exchange = exchange;
            this.routingKey = routingKey;
            this.body = body;
        }

        protected override void ExecuteCore(IModel model, IBasicProperties properties)
        {
            model.BasicPublish(exchange, routingKey, properties, body);
        }
    }
}