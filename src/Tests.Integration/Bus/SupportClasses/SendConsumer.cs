using Roger;

namespace Tests.Integration.Bus.SupportClasses
{
    public class SendConsumer : IConsumer<SendMessage>
    {
        public bool Received;

        public void Consume(SendMessage message)
        {
            Received = true;
        }
    }
}