using Roger;

namespace Tests.Integration.Bus.SupportClasses
{
    public class MyResponseCurrentMessageConsumer : GenericConsumer<MyReply>
    {
        private readonly IRabbitBus bus;
        public CurrentMessageInformation CurrentMessage;

        public MyResponseCurrentMessageConsumer(IRabbitBus bus)
        {
            this.bus = bus;
        }

        public override void Consume(MyReply message)
        {
            CurrentMessage = bus.CurrentMessage;
            base.Consume(message);
        }
    }
}