using System.Threading;
using Roger;

namespace Tests.Integration.Bus.SupportClasses
{
    public class GenericConsumer<T> : IConsumer<T> where T : class
    {
        public T LastReceived;
        private readonly AutoResetEvent delivered = new AutoResetEvent(false);

        public void Consume(T message)
        {
            LastReceived = message;
            delivered.Set();
        }

        public bool WaitForDelivery(int timeout = 1000)
        {
            return delivered.WaitOne(timeout);
        }
    }
}