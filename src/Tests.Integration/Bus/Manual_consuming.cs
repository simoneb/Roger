using MbUnit.Framework;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public class Manual_consuming : With_default_bus
    {
        [Test]
        public void Consume_message()
        {
            var consumer = new GenericConsumer<MyMessage>();
            Bus.AddInstanceSubscription(consumer);

            Bus.Consume(new MyMessage());

            Assert.IsNotNull(consumer.LastReceived);
        }
    }
}