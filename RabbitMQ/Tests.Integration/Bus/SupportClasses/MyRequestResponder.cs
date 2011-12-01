using Rabbus;

namespace Tests.Integration.Bus.SupportClasses
{
    public class MyRequestResponder : IConsumer<MyRequest>
    {
        private readonly IRabbitBus bus;
        public MyRequest Received;

        public MyRequestResponder(IRabbitBus bus)
        {
            this.bus = bus;
        }

        public void Consume(MyRequest message)
        {
            Received = message;
            bus.Reply(new MyReply());
        }
    }
}