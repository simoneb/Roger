using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using RabbitMQ.Client.Events;

namespace Shoveling.Test.FunctionalSpecs
{
    public class IndexedEnumerator : IEnumerator<BasicDeliverEventArgs>
    {
        private readonly ConcurrentQueue<BasicDeliverEventArgs> m_incomingStoreQueue;
        private readonly List<BasicDeliverEventArgs> m_storage;
        private int currentIndex;
        private BasicDeliverEventArgs current;

        public IndexedEnumerator(ConcurrentQueue<BasicDeliverEventArgs> incomingStoreQueue, List<BasicDeliverEventArgs> storage)
        {
            m_incomingStoreQueue = incomingStoreQueue;
            m_storage = storage;
        }

        public void Dispose()
        {
            currentIndex = 0;
        }

        public bool MoveNext()
        {
            Func<int, bool> canAdvance = index => m_storage.Count > index;
            var canAdvanceNow = canAdvance(currentIndex);

            if (canAdvanceNow)
            {
                current = m_storage[currentIndex++];
            }
            else if(!m_incomingStoreQueue.IsEmpty)
            {
                SpinWait.SpinUntil(() => canAdvance(currentIndex));

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