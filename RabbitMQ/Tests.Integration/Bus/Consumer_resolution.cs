using MbUnit.Framework;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public class Consumer_resolution : With_default_bus
    {
        private MyConsumer consumer;

        protected override void BeforeBusInitialization()
        {
            Register(consumer = new MyConsumer());
        }

        [Test]
        public void Publish()
        {
            Bus.Publish(new MyMessage());

            WaitForDelivery();

            Assert.IsNotNull(consumer.Received);
        }
    }
}