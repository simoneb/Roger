using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MbUnit.Framework;
using RabbitMQ.Client.Events;
using Tests.Integration.Utils;

namespace Tests.Integration.FunctionalSpecs
{
    public class Subscription_before_beginning_of_session : With_message_store
    {
        [Test]
        [Row(null, null)]
        [Row(150, null)]
        [Row(null, 150)]
        [Row(300, 150)]
        public void Should_not_lose_messages(ushort? delayBetweenPublishesInMilliseconds, ushort? delayWhenStoringMessage)
        {
            using (storage = new FakeMessageStore(delayWhenStoringMessage))
                Run(delayBetweenPublishesInMilliseconds);
        }

        private void Run(ushort? delayBetweenPublishesInMilliseconds = null)
        {
            storeHalfWay.Set();
            var storeTokens = MessageStore().ToArray();

            var consumerResult = Start<IEnumerable<BasicDeliverEventArgs>>(Consumer);
            var handle = new ManualResetEvent(false);

            var basicDeliverEventArgses = new BasicDeliverEventArgs[0];

            Start(() =>
            {
                basicDeliverEventArgses = consumerResult.Item1.Result.ToArray();
                handle.Set();
            });

            Thread.Sleep(2500);

            Start(() => Producer(delayBetweenPublishesInMilliseconds));

            handle.WaitOne();

            Assert.AreEqual(messageNumber, basicDeliverEventArgses.Length);
            Assert.ForAll(basicDeliverEventArgses.Select(r => r.Source()), source => source == "Producer");

            foreach (var token in storeTokens)
                token.Cancel();
        }
    }
}