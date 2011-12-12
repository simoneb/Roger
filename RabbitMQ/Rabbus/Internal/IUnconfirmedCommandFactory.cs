namespace Rabbus.Internal
{
    internal interface IUnconfirmedCommandFactory : IDeliveryCommandFactory
    {
        bool CanExecute { get; }
    }
}