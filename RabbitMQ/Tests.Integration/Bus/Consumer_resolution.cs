using System.Threading;
using MbUnit.Framework;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public class Consumer_resolution : With_default_bus
    {
        private MyConsumer consumer;

        protected override void BeforeBusInitialization()
        {
            consumer = new MyConsumer();
            ConsumerResolver.Register(consumer);
        }

        [Test]
        public void Publish()
        {
            Bus.Publish(new MyMessage());

            Thread.Sleep(200);

            Assert.IsNotNull(consumer.Received);
        }
    }
}