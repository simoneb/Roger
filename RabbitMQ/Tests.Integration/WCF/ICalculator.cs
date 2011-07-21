using System.ServiceModel;

namespace Tests.Integration.WCF
{
    [ServiceContract]
    public interface ICalculator
    {
        [OperationContract]
        int Add(int a, int b);
    }
}