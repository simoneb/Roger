using System;
using System.Collections.Generic;
using System.Threading;
using Roger;

namespace Tests.Integration.Bus.SupportClasses
{
    public class GenericConsumer<T> : IConsumer<T> where T : class
    {
        public T LastReceived;
        private readonly AutoResetEvent delivered = new AutoResetEvent(false);
        public readonly SynchronizedCollection<T> Received = new SynchronizedCollection<T>();

        public virtual void Consume(T message)
        {
            Received.Add(message);
            LastReceived = message;
            delivered.Set();
        }

        public bool WaitForDelivery(int timeout = 1000)
        {
            return delivered.WaitOne(timeout);
        }

        public bool WaitForDelivery(TimeSpan timeout)
        {
            return delivered.WaitOne(timeout);
        }
    }
}