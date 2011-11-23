using System.Threading;
using Rabbus;

namespace Tests.Integration.Bus.SupportClasses
{
    public class MyConsumer : IConsumer<MyMessage>
    {
        public MyMessage Received;
        private AutoResetEvent m_delivered = new AutoResetEvent(false);

        public void Consume(MyMessage message)
        {
            Received = message;
            m_delivered.Set();
        }

        public void WaitForDelivery()
        {
            m_delivered.WaitOne();
        }
    }
}