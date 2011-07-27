using System;
using Rabbus;

namespace Tests.Integration.Bus.SupportClasses
{
    public class CatchingMyRequestResponder : IConsumer<MyRequest>
    {
        private readonly IRabbitBus bus;

        public CatchingMyRequestResponder(IRabbitBus bus)
        {
            this.bus = bus;
        }

        public void Consume(MyRequest message)
        {
            try
            {
                bus.Reply(new MyResponse());
            }
            catch (Exception e)
            {
                Exception = e;
            }
        }

        public Exception Exception { get; private set; }
    }
}