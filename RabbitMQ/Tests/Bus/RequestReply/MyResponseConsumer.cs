using Rabbus;

namespace Tests.Bus.RequestReply
{
    public class MyResponseConsumer : IConsumer<MyResponse>
    {
        public MyResponse Received;

        public void Consume(MyResponse message)
        {
            Received = message;
        }
    }
}