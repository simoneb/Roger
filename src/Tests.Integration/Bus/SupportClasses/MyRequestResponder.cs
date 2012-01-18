using Roger;

namespace Tests.Integration.Bus.SupportClasses
{
    public class MyRequestResponder : GenericConsumer<MyRequest>
    {
        private readonly IRabbitBus bus;

        public MyRequestResponder(IRabbitBus bus)
        {
            this.bus = bus;
        }

        public override void Consume(MyRequest message)
        {
            bus.Reply(new MyReply());
            base.Consume(message);
        }
    }
}