using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace Shoveling.Test.FunctionalSpecs
{
    public class FakeMessageStore : IMessageStore
    {
        private readonly ushort? m_delayWhenStoringMessage;
        private readonly ConcurrentQueue<BasicDeliverEventArgs> incomingQueue = new ConcurrentQueue<BasicDeliverEventArgs>();
        readonly List<BasicDeliverEventArgs> storage = new List<BasicDeliverEventArgs>();
        private readonly CancellationTokenSource m_cancellation;

        public FakeMessageStore(ushort? delayWhenStoringMessage)
        {
            m_delayWhenStoringMessage = delayWhenStoringMessage;
            m_cancellation = new CancellationTokenSource();
            Task.Factory.StartNew(StoreInternal, m_cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        private void StoreInternal()
        {
            while (true)
            {
                BasicDeliverEventArgs incomingMessage;
                while(incomingQueue.TryDequeue(out incomingMessage))
                {
                    storage.Add(incomingMessage);

                    if (m_delayWhenStoringMessage.HasValue)
                        Thread.Sleep(m_delayWhenStoringMessage.Value);
                }

                Thread.Sleep(10);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<BasicDeliverEventArgs> GetEnumerator()
        {
            return new IndexedEnumerator(incomingQueue, storage);
        }

        public void Store(BasicDeliverEventArgs message)
        {
            incomingQueue.Enqueue(message);
        }

        public int Count { get { return storage.Count; } }

        public void Dispose()
        {
            if (!m_cancellation.IsCancellationRequested)
            {
                m_cancellation.Cancel();
            }
        }
    }
}