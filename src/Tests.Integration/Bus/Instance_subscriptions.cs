using MbUnit.Framework;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public class Instance_subscriptions : With_default_bus
    {
        [Test]
        public void Test_subscription()
        {
            var consumer = new GenericConsumer<MyMessage>();

            Bus.AddInstanceSubscription(consumer);

            Bus.Publish(new MyMessage { Value = 1 });

            WaitForDelivery();

            Assert.AreEqual(1, consumer.LastReceived.Value);
        }

        [Test]
        public void Test_unsubscription()
        {
            var consumer = new GenericConsumer<MyMessage>();

            var token = Bus.AddInstanceSubscription(consumer);

            token.Dispose();

            Bus.Publish(new MyMessage { Value = 1 });

            WaitForDelivery();

            Assert.IsNull(consumer.LastReceived);
        }
    }
}