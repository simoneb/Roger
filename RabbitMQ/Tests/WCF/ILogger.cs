using System.ServiceModel;

namespace Tests.WCF
{
    [ServiceContract]
    public interface ILogger
    {
        [OperationContract(IsOneWay = true)]
        void Log(string message);
    }
}