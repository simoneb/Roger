using System.ServiceModel;
using System.Threading;

namespace Tests.WCF
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