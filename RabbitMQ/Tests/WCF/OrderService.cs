using System.ServiceModel;

namespace Tests.Integration.WCF
{
    [ServiceBehavior]
    public class OrderService : IOrderService
    {
        public void Order()
        {
            var callback = OperationContext.Current.GetCallbackChannel<IOrderCallback>();

            callback.OrderCompleted();
        }
    }
}