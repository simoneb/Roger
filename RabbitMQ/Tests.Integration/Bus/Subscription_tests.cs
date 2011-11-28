using MbUnit.Framework;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public class Subscription_tests : With_default_bus
    {
        [Test]
        public void Test_subscription()
        {
            var consumer = new MyConsumer();

            Bus.AddInstanceSubscription(consumer);

            Bus.Publish(new MyMessage { Value = 1 });

            WaitForDelivery();

            Assert.AreEqual(1, consumer.LastReceived.Value);
        }

        [Test]
        public void Test_unsubscription()
        {
            var consumer = new MyConsumer();

            var token = Bus.AddInstanceSubscription(consumer);

            token.Dispose();

            Bus.Publish(new MyMessage { Value = 1 });

            WaitForDelivery();

            Assert.IsNull(consumer.LastReceived);
        }
    }
}