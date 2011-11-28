using System.Collections.Generic;
using System.Threading;
using Rabbus;

namespace Tests.Integration.Bus.SupportClasses
{
    public class MyThreadCheckingConsumer : IConsumer<MyMessage>
    {
        private readonly IRabbitBus m_bus;
        public readonly SynchronizedCollection<MyMessage> Received = new SynchronizedCollection<MyMessage>();
        readonly AutoResetEvent handle = new AutoResetEvent(false);

        public MyThreadCheckingConsumer(IRabbitBus bus)
        {
            m_bus = bus;
        }

        public void Consume(MyMessage message)
        {
            Received.Add((MyMessage)m_bus.CurrentMessage.Body);
            handle.Set();
        }

        public void WaitUntil(int numberOfMessages)
        {
            while (Received.Count < numberOfMessages)
                handle.WaitOne(100);
        }
    }
}