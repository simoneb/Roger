using Rabbus;

namespace Tests.Integration.Bus
{
    public class MyConsumer : IConsumer<MyMessage>
    {
        public MyMessage Received;

        public void Consume(MyMessage message)
        {
            Received = message;
        }
    }
}