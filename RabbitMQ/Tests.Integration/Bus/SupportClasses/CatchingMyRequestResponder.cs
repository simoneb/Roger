using System;
using Rabbus;

namespace Tests.Integration.Bus.SupportClasses
{
    public class CatchingResponder<TRequest, TReply> : IConsumer<TRequest> where TRequest : class
    {
        private readonly IRabbitBus bus;

        public CatchingResponder(IRabbitBus bus)
        {
            this.bus = bus;
        }

        public void Consume(TRequest message)
        {
            try
            {
                bus.Reply(Activator.CreateInstance<TReply>());
            }
            catch (Exception e)
            {
                Exception = e;
            }
        }

        public Exception Exception { get; private set; }
    }
}