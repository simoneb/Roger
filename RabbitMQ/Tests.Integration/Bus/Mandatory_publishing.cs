using System.Threading;
using MbUnit.Framework;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public class Mandatory_publishing : With_default_bus
    {
        [Test]
        public void PublishMandatory_should_invoke_delivery_failure_callback_when_no_consumers()
        {
            var handle = new ManualResetEvent(false);

            Bus.PublishMandatory(new MyMessage {Value = 1}, reason => handle.Set());

            if(!handle.WaitOne(100))
                Assert.Fail("Delivery failure callback was not called");
        }

        [Test]
        public void PublishMandatory_should_work_when_message_deliverable()
        {
            var consumer = new MyConsumer();

            Bus.AddInstanceSubscription(consumer);

            WaitForDelivery();

            var handle = new ManualResetEvent(false);

            Bus.PublishMandatory(new MyMessage {Value = 1}, reason => handle.Set());

            if(handle.WaitOne(100))
                Assert.Fail("Delivery failure callback wasn't expected to be called");

            Assert.IsNotNull(consumer.Received);
        }
    }
}