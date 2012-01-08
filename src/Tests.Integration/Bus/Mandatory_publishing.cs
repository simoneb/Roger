using System.Threading;
using MbUnit.Framework;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public class Mandatory_publishing : With_default_bus
    {
        [Test]
        public void Should_invoke_delivery_failure_callback_when_no_consumers()
        {
            var handle = new ManualResetEvent(false);

            Bus.PublishMandatory(new MyMessage {Value = 1}, reason => handle.Set());

            if(!handle.WaitOne(1000))
                Assert.Fail("Delivery failure callback was not called");
        }

        [Test]
        public void Should_invoke_delivery_failure_only_when_failing_in_the_same_context()
        {
            var handle = new CountdownEvent(2);

            int first = 0;
            int second = 0;

            Bus.PublishMandatory(new MyMessage {Value = 1}, reason => { first++; handle.Signal(); });
            Bus.PublishMandatory(new MyMessage {Value = 1}, reason => { second++; handle.Signal(); });

            handle.Wait(2000);

            Assert.AreEqual(1, first);
            Assert.AreEqual(1, second);
        }

        [Test]
        public void Should_work_when_message_deliverable()
        {
            var consumer = new GenericConsumer<MyMessage>();

            Bus.AddInstanceSubscription(consumer);

            WaitForDelivery();

            var handle = new ManualResetEvent(false);

            Bus.PublishMandatory(new MyMessage {Value = 1}, reason => handle.Set());

            if(handle.WaitOne(1000))
                Assert.Fail("Delivery failure callback wasn't expected to be called");

            Assert.IsNotNull(consumer.LastReceived);
        }
    }
}