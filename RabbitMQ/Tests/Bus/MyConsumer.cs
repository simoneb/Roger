using Rabbus;

namespace Tests.Bus
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