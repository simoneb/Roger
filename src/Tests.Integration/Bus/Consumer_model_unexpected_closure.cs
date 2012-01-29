using System;
using System.Linq;
using System.Threading;
using MbUnit.Framework;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public class Consumer_model_unexpected_closure : With_default_bus
    {
        private SlowConsumer<MyMessage> consumer;
        private const int ConsumerDelayMs = 100;

        protected override void BeforeBusInitialization()
        {
            RegisterOnDefaultBus(consumer = new SlowConsumer<MyMessage>(TimeSpan.FromMilliseconds(ConsumerDelayMs), 10));
            base.BeforeBusInitialization();
        }

        [Test]
        public void When_consumer_model_is_closed_unexpectedly_it_should_be_restored_and_receive_all_messages_in_sequence()
        {
            for (int i = 0; i < 100; i++)
                Bus.Publish(new MyMessage { Value = i });

            Assert.IsTrue(consumer.WaitForDelivery(2 * ConsumerDelayMs * 10, 90));

            Bus.Consumer.Model.Close();

            consumer.WaitForDelivery(2 * ConsumerDelayMs * 90, 50);

            for (int i = 100; i < 150; i++)
                Bus.Publish(new MyMessage { Value = i });

            consumer.WaitForDelivery(2 * ConsumerDelayMs * 50);

            Assert.AreElementsEqual(Enumerable.Range(0, 150), consumer.Received.Select(r => r.Value));
        }
    }
}