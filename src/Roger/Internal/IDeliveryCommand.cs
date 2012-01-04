using RabbitMQ.Client;

namespace Rabbus.Internal
{
    internal interface IDeliveryCommand
    {
        void Execute(IModel model, RabbusEndpoint endpoint, IBasicReturnHandler basicReturnHandler);
    }
}