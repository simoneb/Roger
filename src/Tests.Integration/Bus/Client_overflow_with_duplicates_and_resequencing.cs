using System;
using System.Linq;
using System.Threading;
using MbUnit.Framework;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public class Client_overflow_with_duplicates_and_resequencing : With_bus_on_secondary
    {
        private SlowConsumer<MyMessage> consumer;
        private const int ConsumerDelayMs = 100;

        protected override void BeforeSecondaryBusInitialization()
        {
            RegisterOnSecondaryBus(consumer = new SlowConsumer<MyMessage>(TimeSpan.FromMilliseconds(ConsumerDelayMs), 10));
            base.BeforeSecondaryBusInitialization();
        }

        [Test]
        public void When_client_link_goes_down_and_restores_it_should_receive_all_messages_in_sequence()
        {
            for (int i = 0; i < 100; i++)
                Bus.Publish(new MyMessage {Value = i});

            Assert.IsTrue(consumer.WaitForDelivery(2 * ConsumerDelayMs * 10, 90));

            StopAlternativePortProxy();
            Thread.Sleep(100);
            StartAlternativePortProxy();

            consumer.WaitForDelivery((int) (SecondaryBus.ConnectionAttemptInterval.TotalMilliseconds + 2 * ConsumerDelayMs * 90), 50);

            for (int i = 100; i < 150; i++)
                Bus.Publish(new MyMessage { Value = i });

            consumer.WaitForDelivery((int)(SecondaryBus.ConnectionAttemptInterval.TotalMilliseconds + 2 * ConsumerDelayMs * 50));

            Assert.AreElementsEqual(Enumerable.Range(0, 150), consumer.Received.Select(r => r.Value));
        }
    }
}