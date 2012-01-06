using Roger;

namespace Tests.Integration.Bus.SupportClasses
{
    public class SendCurrentMessageConsumer : IConsumer<SendMessage>
    {
        private readonly IRabbitBus bus;
        public CurrentMessageInformation CurrentMessage;

        public SendCurrentMessageConsumer(IRabbitBus bus)
        {
            this.bus = bus;
        }

        public void Consume(SendMessage message)
        {
            CurrentMessage = bus.CurrentMessage;
        }
    }
}