using System;
using System.Threading;
using MbUnit.Framework;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public class Broker_shutdown_recovery : With_default_bus
    {
        private GenericConsumer<MyMessage> consumer;

        protected override void BeforeBusInitialization()
        {
            Register(consumer = new GenericConsumer<MyMessage>());
        }

        [Test]
        public void Should_be_able_to_publish_messages_after_recovery()
        {
            Bus.Publish(new MyMessage {Value = 1});
            
            Assert.IsTrue(consumer.WaitForDelivery(), "Message was not delivered in time");

            SafelyShutDownBroker();
            RestartBrokerAndWaitForConnectionRecovery();

            Bus.Publish(new MyMessage {Value = 2});
            
            Assert.IsTrue(consumer.WaitForDelivery(), "Message was not delivered in time");

            Assert.IsNotNull(consumer.LastReceived);
            Assert.AreEqual(2, consumer.LastReceived.Value);
        }

        [Test]
        public void Should_be_able_to_enqueue_message_during_broker_failure_and_perform_publish_once_back_online()
        {
            SafelyShutDownBroker();

            Bus.Publish(new MyMessage { Value = 1 });

            RestartBrokerAndWaitForConnectionRecovery();

            Assert.IsTrue(consumer.WaitForDelivery(), "Message was not delivered in time");

            Assert.IsNotNull(consumer.LastReceived);
            Assert.AreEqual(1, consumer.LastReceived.Value);
        }

        private void RestartBrokerAndWaitForConnectionRecovery()
        {
            Broker.StartBrokerApplication();
            Thread.Sleep(Bus.ConnectionAttemptInterval + TimeSpan.FromSeconds(1));
        }

        private static void SafelyShutDownBroker()
        {
            Broker.StopBrokerApplication();
        }
    }
}