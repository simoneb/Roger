using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using RabbitMQ.Client.Events;

namespace Tests.Integration.FunctionalSpecs
{
    public class IndexedEnumerator : IEnumerator<BasicDeliverEventArgs>
    {
        private readonly ConcurrentQueue<BasicDeliverEventArgs> m_incomingStoreQueue;
        private readonly List<BasicDeliverEventArgs> m_storage;
        private int currentIndex;
        private BasicDeliverEventArgs current;
        private readonly Func<int, bool> m_canAdvance;

        public IndexedEnumerator(ConcurrentQueue<BasicDeliverEventArgs> incomingStoreQueue, List<BasicDeliverEventArgs> storage)
        {
            m_incomingStoreQueue = incomingStoreQueue;
            m_storage = storage;
            m_canAdvance = index => m_storage.Count > index;
        }

        public void Dispose()
        {
            currentIndex = 0;
        }

        public bool MoveNext()
        {
            var canAdvanceNow = m_canAdvance(currentIndex);

            if (canAdvanceNow)
            {
                current = m_storage[currentIndex++];
            }
            else if(!m_incomingStoreQueue.IsEmpty)
            {
                SpinWait.SpinUntil(() => m_canAdvance(currentIndex));

                current = m_storage[currentIndex++];
                return true;
            }

            return canAdvanceNow;
        }

        public void Reset()
        {
            currentIndex = 0;
        }

        public BasicDeliverEventArgs Current { get { return current; } }

        object IEnumerator.Current { get { return Current; } }
    }
}