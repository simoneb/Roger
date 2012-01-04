using System.Collections.Concurrent;
using System.Threading;
using Rabbus;

namespace Tests.Integration.Bus.SupportClasses
{
    public class GenericMultipleArrivalsConsumer<T> : IConsumer<T> where T : class
    {
        private readonly CountdownEvent delivered;
        private readonly ConcurrentQueue<T> received = new ConcurrentQueue<T>();

        public GenericMultipleArrivalsConsumer(int expectedArrivals)
        {
            delivered = new CountdownEvent(expectedArrivals);
        }

        public ConcurrentQueue<T> Received
        {
            get { return received; }
        }

        public void Consume(T message)
        {
            Received.Enqueue(message);
            delivered.Signal();
        }

        public bool WaitForDelivery(int timeout = 1000)
        {
            return delivered.Wait(timeout);
        }
    }
}