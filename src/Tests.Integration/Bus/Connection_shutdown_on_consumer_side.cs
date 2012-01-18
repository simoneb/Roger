using System;
using System.Threading;
using MbUnit.Framework;
using Tests.Integration.Bus.SupportClasses;
using Tests.Integration.Utils;

namespace Tests.Integration.Bus
{
    public class Connection_shutdown_on_consumer_side : With_bus_on_secondary
    {
        private GenericConsumer<MyMessage> consumer;

        protected override void BeforeSecondaryBusInitialization()
        {
            RegisterOnSecondaryBus(consumer = new GenericConsumer<MyMessage>());
        }

        [Test]
        [Description("Just to check that everything's up and running")]
        public void Consumer_on_secondary_should_receive_message()
        {
            Bus.Publish(new MyMessage());

            Assert.IsTrue(consumer.WaitForDelivery());
        }

        [Test]
        public void Consumer_should_receive_message_published_while_its_network_link_was_down()
        {
            Bootstrap.StopSecondaryConnectionLink();

            Thread.Sleep(100);

            Bus.Publish(new MyMessage());

            Thread.Sleep(100);

            Bootstrap.StartSecondaryConnectionLink();

            Assert.IsTrue(consumer.WaitForDelivery(SecondaryBus.ConnectionAttemptInterval + TimeSpan.FromSeconds(1)));
        }
    }
}