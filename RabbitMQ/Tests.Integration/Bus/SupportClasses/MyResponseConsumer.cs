using Rabbus;

namespace Tests.Integration.Bus.SupportClasses
{
    public class MyResponseConsumer : IConsumer<MyReply>
    {
        public MyReply Received;

        public void Consume(MyReply message)
        {
            Received = message;
        }
    }
}