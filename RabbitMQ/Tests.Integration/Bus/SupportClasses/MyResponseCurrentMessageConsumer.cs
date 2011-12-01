using Rabbus;

namespace Tests.Integration.Bus.SupportClasses
{
    public class MyResponseCurrentMessageConsumer : IConsumer<MyReply>
    {
        private readonly IRabbitBus bus;
        public CurrentMessageInformation CurrentMessage;

        public MyResponseCurrentMessageConsumer(IRabbitBus bus)
        {
            this.bus = bus;
        }

        public void Consume(MyReply message)
        {
            CurrentMessage = bus.CurrentMessage;
        }
    }
}