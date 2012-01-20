using System.ServiceModel;

namespace Tests.Integration.Exploratory.WCF
{
    [ServiceContract]
    public interface IOrderCallback
    {
        [OperationContract(IsOneWay = true)]
        void OrderCompleted();
    }
}