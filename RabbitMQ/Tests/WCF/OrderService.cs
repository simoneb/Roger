using System.ServiceModel;

namespace Tests.WCF
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