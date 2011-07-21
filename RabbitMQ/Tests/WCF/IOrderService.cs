using System.ServiceModel;

namespace Tests.Integration.WCF
{
    [ServiceContract(CallbackContract = typeof(IOrderCallback))]
    public interface IOrderService
    {
        [OperationContract]
        void Order();
    }
}