using System.Collections.Generic;
using System.Threading;
using Roger;

namespace Tests.Integration.Bus.SupportClasses
{
    public class MyThreadCheckingConsumer : IConsumer<MyMessage>
    {
        private readonly IRabbitBus bus;
        public readonly SynchronizedCollection<MyMessage> Received = new SynchronizedCollection<MyMessage>();
        private readonly CountdownEvent handle;

        public MyThreadCheckingConsumer(IRabbitBus bus, int numberOfMessages)
        {
            this.bus = bus;
            handle = new CountdownEvent(numberOfMessages);
        }

        public void Consume(MyMessage message)
        {
            Received.Add((MyMessage)bus.CurrentMessage.Body);
            handle.Signal();
        }

        public bool WaitUntilDelivery(int timeout)
        {
            return handle.Wait(timeout);
        }
    }
}