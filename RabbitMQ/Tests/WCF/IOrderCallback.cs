using System.ServiceModel;

namespace Tests.WCF
{
    [ServiceContract]
    public interface IOrderCallback
    {
        [OperationContract(IsOneWay = true)]
        void OrderCompleted();
    }
}