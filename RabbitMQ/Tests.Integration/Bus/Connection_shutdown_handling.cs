using System;
using System.Diagnostics;
using System.Threading;
using MbUnit.Framework;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public class Connection_shutdown_handling : With_default_bus
    {
        private MyConsumer consumer;

        protected override void BeforeBusInitialization()
        {
            Register(consumer = new MyConsumer());
        }

        [Test]
        public void Should_handle_exception_gracefully_and_retry_connection()
        {
            Thread.Sleep(1000);
            Broker.StopApp();
            Debug.WriteLine("Stopped app");
            Thread.Sleep(1000);

            Broker.StartAppAndWait();
            Debug.WriteLine("Started app");

            // wait for reconnection
            Thread.Sleep(Bus.ConnectionAttemptInterval + TimeSpan.FromSeconds(1));

            Bus.Publish(new MyMessage());

            WaitForDelivery();

            Assert.IsNotNull(consumer.Received);
        }
    }
}