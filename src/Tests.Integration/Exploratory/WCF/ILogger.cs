using System.ServiceModel;

namespace Tests.Integration.Exploratory.WCF
{
    [ServiceContract]
    public interface ILogger
    {
        [OperationContract(IsOneWay = true)]
        void Log(string message);
    }
}