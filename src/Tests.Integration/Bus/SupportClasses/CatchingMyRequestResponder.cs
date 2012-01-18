using System;
using Roger;

namespace Tests.Integration.Bus.SupportClasses
{
    public class CatchingResponder<TRequest, TReply> : GenericConsumer<TRequest> where TRequest : class
    {
        private readonly IRabbitBus bus;

        public CatchingResponder(IRabbitBus bus)
        {
            this.bus = bus;
        }

        public override void Consume(TRequest message)
        {
            try
            {
                bus.Reply(Activator.CreateInstance<TReply>());
            }
            catch (Exception e)
            {
                Exception = e;
            }

            base.Consume(message);
        }

        public Exception Exception { get; private set; }
    }
}