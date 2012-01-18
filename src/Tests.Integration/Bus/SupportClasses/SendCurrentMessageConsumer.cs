using Roger;

namespace Tests.Integration.Bus.SupportClasses
{
    public class SendCurrentMessageConsumer : GenericConsumer<SendMessage>
    {
        private readonly IRabbitBus bus;
        public CurrentMessageInformation CurrentMessage;

        public SendCurrentMessageConsumer(IRabbitBus bus)
        {
            this.bus = bus;
        }

        public override void Consume(SendMessage message)
        {
            CurrentMessage = bus.CurrentMessage;

            base.Consume(message);
        }
    }
}