using System;
using System.Linq;
using System.Threading;
using Common;
using MbUnit.Framework;
using Roger;
using Tests.Integration.Bus.SupportClasses;
using Tests.Integration.Utils;

namespace Tests.Integration.Bus
{
    public abstract class With_bus_on_secondary : With_default_bus
    {
        protected RogerBus SecondaryBus;
        private SimpleConsumerContainer secondaryConsumerContainer;

        protected override void AfterBusInitialization()
        {
            secondaryConsumerContainer = new SimpleConsumerContainer();

            BeforeSecondaryBusInitialization();
            
            SecondaryBus = new RogerBus(new ManualConnectionFactory(Helpers.CreateSecondaryConnectionToMainVirtualHost),
                                               secondaryConsumerContainer);

            SecondaryBus.Start();

            AfterSecondaryBusInitialization();
        }

        protected void RegisterOnSecondaryBus(IConsumer consumer)
        {
            secondaryConsumerContainer.Register(consumer);
        }

        protected virtual void AfterSecondaryBusInitialization()
        {
            
        }

        protected virtual void BeforeSecondaryBusInitialization()
        {
            
        }

        protected override void AfterBusDispose()
        {
            SecondaryBus.Dispose();
        }
    }

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

            Bootstrap.StopSecondaryConnectionLink();
            Thread.Sleep(100);
            Bootstrap.StartSecondaryConnectionLink();

            consumer.WaitForDelivery((int) (SecondaryBus.ConnectionAttemptInterval.TotalMilliseconds + 2 * ConsumerDelayMs * 90), 50);

            for (int i = 100; i < 150; i++)
                Bus.Publish(new MyMessage { Value = i });

            consumer.WaitForDelivery((int)(SecondaryBus.ConnectionAttemptInterval.TotalMilliseconds + 2 * ConsumerDelayMs * 50));

            Assert.AreElementsEqual(Enumerable.Range(0, 150), consumer.Received.Select(r => r.Value));
        }
    }

    public class SlowConsumer<T> : GenericMultipleArrivalsConsumer<MyMessage>
    {
        private readonly TimeSpan delay;

        public SlowConsumer(TimeSpan delay, int expectedDeliveries) : base(expectedDeliveries)
        {
            this.delay = delay;
        }

        public override void Consume(MyMessage message)
        {
            base.Consume(message);
            Thread.Sleep(delay);
        }
    }
}