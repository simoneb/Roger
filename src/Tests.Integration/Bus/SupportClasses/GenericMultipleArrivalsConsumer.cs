using System;
using System.Collections.Generic;
using System.Threading;
using Roger;

namespace Tests.Integration.Bus.SupportClasses
{
    public class GenericMultipleArrivalsConsumer<T> : IConsumer<T> where T : class
    {
        private CountdownEvent delivered;
        public readonly SynchronizedCollection<T> Received = new SynchronizedCollection<T>();

        public GenericMultipleArrivalsConsumer(int expectedArrivals)
        {
            delivered = new CountdownEvent(expectedArrivals);
        }

        private void SetExpectedArrivals(int expectedArrivals)
        {
            delivered = new CountdownEvent(expectedArrivals);
        }

        public virtual void Consume(T message)
        {
            Received.Add(message);
            delivered.Signal();
        }

        public bool WaitForDelivery(int timeoutMs = 1000, int newCountToExpectOn = 0)
        {
            var success = delivered.Wait(timeoutMs);

            if(success && newCountToExpectOn > 0)
                SetExpectedArrivals(newCountToExpectOn);

            return success;
        }

        public bool WaitForDelivery(TimeSpan timeout)
        {
            return delivered.Wait(timeout);
        }
    }
}