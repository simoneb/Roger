using Rabbus;

namespace Tests.Integration.Bus.RequestReply
{
    public class MyResponseCurrentMessageConsumer : IConsumer<MyResponse>
    {
        private readonly IRabbitBus bus;
        public CurrentMessageInformation CurrentMessage;

        public MyResponseCurrentMessageConsumer(IRabbitBus bus)
        {
            this.bus = bus;
        }

        public void Consume(MyResponse message)
        {
            CurrentMessage = bus.CurrentMessage;
        }
    }
}