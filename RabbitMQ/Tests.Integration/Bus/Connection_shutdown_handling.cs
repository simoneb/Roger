using System;
using System.Diagnostics;
using System.Threading;
using MbUnit.Framework;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public class Connection_shutdown_handling : With_default_bus
    {
        private MyConsumer m_consumer;

        protected override void BeforeBusInitialization()
        {
            Register(m_consumer = new MyConsumer());
        }

        [Test]
        public void Should_handle_exception_gracefully_and_retry_connection()
        {
            SafelyShutDownBroker();
            RestartBrokerAndWait();

            Bus.Publish(new MyMessage());
            m_consumer.WaitForDelivery();

            Assert.IsNotNull(m_consumer.LastReceived);
        }

        [Test]
        public void Should_be_able_to_publish_messages_after_recovery()
        {
            Bus.Publish(new MyMessage {Value = 1});
            m_consumer.WaitForDelivery();

            SafelyShutDownBroker();
            RestartBrokerAndWait();

            Bus.Publish(new MyMessage {Value = 2});
            m_consumer.WaitForDelivery();

            Assert.IsNotNull(m_consumer.LastReceived);
            Assert.AreEqual(2, m_consumer.LastReceived.Value);
        }

        private void RestartBrokerAndWait()
        {
            Broker.StartAppAndWait();
            Thread.Sleep(Bus.ConnectionAttemptInterval + TimeSpan.FromSeconds(1));
        }

        private static void SafelyShutDownBroker()
        {
            Thread.Sleep(1000);
            Broker.StopApp();
            Thread.Sleep(1000);
        }
    }
}