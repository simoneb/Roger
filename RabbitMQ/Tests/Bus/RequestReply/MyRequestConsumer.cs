using Rabbus;

namespace Tests.Integration.Bus.RequestReply
{
    public class MyRequestConsumer : IConsumer<MyRequest>
    {
        private readonly IRabbitBus bus;
        public MyRequest Received;

        public MyRequestConsumer(IRabbitBus bus)
        {
            this.bus = bus;
        }

        public void Consume(MyRequest message)
        {
            Received = message;
            bus.Reply(new MyResponse());
        }
    }
}