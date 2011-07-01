using System.ServiceModel;

namespace Tests.WCF
{
    [ServiceContract(CallbackContract = typeof(IOrderCallback))]
    public interface IOrderService
    {
        [OperationContract]
        void Order();
    }
}