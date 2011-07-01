using System.Collections.Generic;
using System.Linq;
using MbUnit.Framework;
using RabbitMQ.Client.Events;

namespace Tests.FunctionalSpecs
{
    public class Subscription_after_beginning_of_session : With_message_store
    {
        [Test]
        [Row(null, null)]
        [Row(150, null)]
        [Row(null, 150)]
        [Row(300, 150)]
        public void Should_not_loose_messages(ushort? delayBetweenPublishesInMilliseconds, ushort? delayWhenStoringMessage)
        {
            using(storage = new FakeMessageStore(delayWhenStoringMessage))
                Run(delayBetweenPublishesInMilliseconds);
        }

        private void Run(ushort? delayBetweenPublishesInMilliseconds = null)
        {
            var storeTokens = MessageStore().ToArray();
            Start(() => Producer(delayBetweenPublishesInMilliseconds));

            var consumerResult = Start<IEnumerable<BasicDeliverEventArgs>>(Consumer);

            Assert.AreEqual(messageNumber, consumerResult.Item1.Result.Count());

            foreach (var token in storeTokens)
                token.Cancel();
        }
    }
}