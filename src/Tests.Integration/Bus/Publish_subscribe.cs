using MbUnit.Framework;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public class Publish_subscribe : With_default_bus
    {
        private GenericConsumer<MyMessage> simpleConsumer;
        private GenericConsumer<MyBaseMessage> baseConsumer;

        protected override void BeforeBusInitialization()
        {
            Register(simpleConsumer = new GenericConsumer<MyMessage>());
            Register(baseConsumer = new GenericConsumer<MyBaseMessage>());
        }

        [Test]
        public void Simple_publish()
        {
            Bus.Publish(new MyMessage {Value = 1});

            Assert.IsTrue(simpleConsumer.WaitForDelivery());
            Assert.AreEqual(1, simpleConsumer.LastReceived.Value);
        }

        [Test]
        public void Publish_on_consumer_of_base()
        {
            Bus.Publish(new MyDerivedMessage { Value = 1 });

            Assert.IsTrue(baseConsumer.WaitForDelivery());
            Assert.AreEqual(1, baseConsumer.LastReceived.Value);
        }
    }
}