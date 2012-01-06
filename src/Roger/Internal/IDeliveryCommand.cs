using RabbitMQ.Client;

namespace Roger.Internal
{
    internal interface IDeliveryCommand
    {
        void Execute(IModel model, RogerEndpoint endpoint, IBasicReturnHandler basicReturnHandler);
    }
}