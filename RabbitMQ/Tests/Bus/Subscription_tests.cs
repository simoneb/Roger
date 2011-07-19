using System.Threading;
using MbUnit.Framework;

namespace Tests.Bus
{
    public class Subscription_tests : With_default_bus
    {
        [Test]
        public void Test_subscription()
        {
            var consumer = new MyConsumer();

            Bus.AddInstanceSubscription(consumer);

            Thread.Sleep(100);

            Bus.Publish(new MyMessage { Value = 1 });

            Thread.Sleep(100);

            Assert.AreEqual(1, consumer.Received.Value);
        }

        [Test]
        public void Test_unsubscription()
        {
            var consumer = new MyConsumer();

            var token = Bus.AddInstanceSubscription(consumer);

            Thread.Sleep(100);

            token.Dispose();

            Bus.Publish(new MyMessage { Value = 1 });

            Thread.Sleep(100);

            Assert.IsNull(consumer.Received);
        }
    }
}