using System.Threading;
using MbUnit.Framework;
using Tests.Integration.Bus.SupportClasses;
using System.Linq;

namespace Tests.Integration.Bus
{
    public class Consume_threading : With_default_bus
    {
        private MyThreadCheckingConsumer consumer;
        private readonly ManualResetEvent publisherHalfway = new ManualResetEvent(false);

        protected override void BeforeBusInitialization()
        {
            Register(consumer = new MyThreadCheckingConsumer(Bus, 200));
        }

        [Test]
        public void Consuming_should_be_thread_safe()
        {
            var t1 = new Thread(Publish100Messages);
            var t2 = new Thread(Consume100Messages);
            t1.Start();
            t2.Start();

            Assert.IsTrue(consumer.WaitUntilDelivery(1000));
            Assert.AreEqual(200, consumer.Received.Count);
            Assert.AreElementsEqualIgnoringOrder(Enumerable.Range(0, 200), consumer.Received.Select(r => r.Value));
        }

        private void Publish100Messages(object _)
        {
            for (int i = 0; i < 100; i++)
            {
                Bus.Publish(new MyMessage {Value = i});
                if (i == 50)
                    publisherHalfway.Set();
            }
        }

        private void Consume100Messages(object _)
        {
            for (int i = 100; i < 200; i++)
            {
                publisherHalfway.WaitOne();
                Bus.Consume(new MyMessage { Value = i });
            }
        }
    }
}