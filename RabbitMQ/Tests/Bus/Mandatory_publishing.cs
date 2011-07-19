using System.Threading;
using MbUnit.Framework;

namespace Tests.Bus
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

            Bus.Subscribe(consumer);

            Thread.Sleep(100);

            var handle = new ManualResetEvent(false);

            Bus.PublishMandatory(new MyMessage {Value = 1}, reason => handle.Set());

            if(handle.WaitOne(100))
                Assert.Fail("Delivery failure callback wasn't expected to be called");

            Assert.IsNotNull(consumer.Received);
        }
    }
}