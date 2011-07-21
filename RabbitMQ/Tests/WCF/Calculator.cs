using System.ServiceModel;

namespace Tests.Integration.WCF
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class Calculator : ICalculator
    {
        public int Add(int a, int b)
        {
            return a + b;
        }
    }
}