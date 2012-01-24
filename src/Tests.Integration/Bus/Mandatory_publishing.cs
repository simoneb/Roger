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

            Bus.PublishMandatory(new MyNeverSubscribedToMessage {Value = 1}, reason => handle.Set());

            if(!handle.WaitOne(100))
                Assert.Fail("Delivery failure callback was not called");
        }

        [Test]
        public void Should_invoke_delivery_failure_only_when_failing_in_the_same_context()
        {
            var handle = new CountdownEvent(2);

            var first = false;
            var second = false;

            Bus.PublishMandatory(new MyNeverSubscribedToMessage {Value = 1}, reason => { first = true; handle.Signal(); });
            Bus.PublishMandatory(new MyNeverSubscribedToMessage { Value = 2 }, reason => { second = true; handle.Signal(); });

            if(!handle.Wait(100))
                Assert.Fail("Delivery failure callbacks were not called");

            Assert.IsTrue(first);
            Assert.IsTrue(second);
        }

        [Test]
        public void Should_work_when_message_deliverable()
        {
            var consumer = new GenericConsumer<MyMessage>();

            Bus.AddInstanceSubscription(consumer);

            consumer.WaitForDelivery();

            var handle = new ManualResetEvent(false);

            Bus.PublishMandatory(new MyMessage {Value = 1}, reason => handle.Set());

            if(handle.WaitOne(1000))
                Assert.Fail("Delivery failure callback wasn't expected to be called");

            Assert.IsNotNull(consumer.LastReceived);
        }
    }
}