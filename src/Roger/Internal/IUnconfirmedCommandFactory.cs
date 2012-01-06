namespace Roger.Internal
{
    internal interface IUnconfirmedCommandFactory : IDeliveryCommandFactory
    {
        bool CanExecute { get; }
    }
}