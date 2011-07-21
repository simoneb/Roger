using System.ServiceModel;

namespace Tests.Integration.WCF
{
    [ServiceContract]
    public interface IOrderCallback
    {
        [OperationContract(IsOneWay = true)]
        void OrderCompleted();
    }
}