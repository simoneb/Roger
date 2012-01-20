using System.ServiceModel;

namespace Tests.Integration.Exploratory.WCF
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