using System;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal abstract class MandatoryCommand : IDeliveryCommand
    {
        public abstract void Execute(IModel model, RogerEndpoint endpoint, IBasicReturnHandler basicReturnHandler);

        protected void PublishMandatory(IModel model, IBasicProperties properties, Action<BasicReturn> basicReturnCallback, string routingKey, string exchange, byte[] body, IBasicReturnHandler basicReturnHandler)
        {
            // todo: handle this, we don't want to subscribe multiple times in case of republish
            if (basicReturnCallback != null)
                basicReturnHandler.Subscribe(new RogerGuid(properties.MessageId), basicReturnCallback);

            model.BasicPublish(exchange, routingKey, true, false, properties, body);
        }
    }
}