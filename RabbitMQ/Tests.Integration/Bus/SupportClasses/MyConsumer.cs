using System.Threading;
using Rabbus;

namespace Tests.Integration.Bus.SupportClasses
{
    public class MyConsumer : IConsumer<MyMessage>
    {
        public MyMessage LastReceived;
        private readonly AutoResetEvent delivered = new AutoResetEvent(false);

        public void Consume(MyMessage message)
        {
            LastReceived = message;
            delivered.Set();
        }

        public void WaitForDelivery()
        {
            delivered.WaitOne();
        }
    }
}