using System.ServiceModel;

namespace Tests.WCF
{
    [ServiceContract]
    public interface ICalculator
    {
        [OperationContract]
        int Add(int a, int b);
    }
}