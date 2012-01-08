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
        public void Should_handle_exception_gracefully_and_retry_connection()
        {
            SafelyShutDownBroker();
            RestartBrokerAndWaitForConnectionRecovery();

            Bus.Publish(new MyMessage());
            consumer.WaitForDelivery();

            Assert.IsNotNull(consumer.LastReceived);
        }

        [Test]
        public void Should_be_able_to_publish_messages_after_recovery()
        {
            Bus.Publish(new MyMessage {Value = 1});
            consumer.WaitForDelivery();

            SafelyShutDownBroker();
            RestartBrokerAndWaitForConnectionRecovery();

            Bus.Publish(new MyMessage {Value = 2});
            consumer.WaitForDelivery();

            Assert.IsNotNull(consumer.LastReceived);
            Assert.AreEqual(2, consumer.LastReceived.Value);
        }

        [Test]
        public void Should_be_able_to_publish_message_during_broker_failure_and_perform_publish_once_back_online()
        {
            SafelyShutDownBroker();

            Bus.Publish(new MyMessage { Value = 1 });

            RestartBrokerAndWaitForConnectionRecovery();

            consumer.WaitForDelivery();

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
            Thread.Sleep(1000);
            Broker.StopBrokerApplication();
            Thread.Sleep(1000);
        }
    }
}