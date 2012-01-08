using System;
using System.Threading;
using Common;
using MbUnit.Framework;
using Roger;
using Tests.Integration.Bus.SupportClasses;
using Tests.Integration.Utils;

namespace Tests.Integration.Bus
{
    public class Connection_shutdown_on_consumer_side : With_default_bus
    {
        private DefaultRogerBus secondaryBus;
        private GenericConsumer<MyMessage> consumer;

        protected override void AfterBusInitialization()
        {
            var container = new SimpleConsumerContainer();
            container.Register(consumer = new GenericConsumer<MyMessage>());
            secondaryBus = new DefaultRogerBus(new IdentityConnectionFactory(Helpers.CreateSecondaryConnectionToMainVirtualHost),
                                               container,
                                               log: Log);
            secondaryBus.Start();
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

            Assert.IsTrue(consumer.WaitForDelivery(secondaryBus.ConnectionAttemptInterval + TimeSpan.FromSeconds(1)));
        }

        [TearDown]
        public void TearDown()
        {
            secondaryBus.Dispose();
        }
    }
}