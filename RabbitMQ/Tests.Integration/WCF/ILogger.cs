using System.ServiceModel;

namespace Tests.Integration.WCF
{
    [ServiceContract]
    public interface ILogger
    {
        [OperationContract(IsOneWay = true)]
        void Log(string message);
    }
}