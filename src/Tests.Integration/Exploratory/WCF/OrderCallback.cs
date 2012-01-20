using System.ServiceModel;
using System.Threading;

namespace Tests.Integration.Exploratory.WCF
{
    [ServiceBehavior]
    public class OrderCallback : IOrderCallback
    {
        public bool Completed;
        public readonly EventWaitHandle Semaphore = new AutoResetEvent(false);

        public void OrderCompleted()
        {
            Completed = true;
            Semaphore.Set();
        }
    }
}