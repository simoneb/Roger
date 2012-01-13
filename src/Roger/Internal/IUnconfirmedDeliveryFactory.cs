namespace Roger.Internal
{
    internal interface IUnconfirmedDeliveryFactory : IDeliveryFactory
    {
        bool CanExecute { get; }
    }
}