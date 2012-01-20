using System.ServiceModel;

namespace Tests.Integration.Exploratory.WCF
{
    [ServiceContract(CallbackContract = typeof(IOrderCallback))]
    public interface IOrderService
    {
        [OperationContract]
        void Order();
    }
}