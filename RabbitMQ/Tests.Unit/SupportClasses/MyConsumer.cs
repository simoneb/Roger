using Rabbus;

namespace Tests.Unit.SupportClasses
{
    public class MyConsumer : IConsumer<MyMessage>
    {
        public bool Consumed;

        public void Consume(MyMessage message)
        {
            Consumed = true;
        }
    }
}