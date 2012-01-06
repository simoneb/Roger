using System.Collections.Generic;
using System.Linq;
using MbUnit.Framework;
using Roger;
using Roger.Internal.Impl;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public class Message_ordering : With_default_bus
    {
        private static readonly uint[] Sequence = new uint[] { 1, 3, 4, 2 };

        protected override IEnumerable<IMessageFilter> MessageFilters
        {
            get { yield return new ResequencingFilter(); }
        }

        protected override ISequenceGenerator SequenceGenerator
        {
            get { return new FakeSequenceGenerator(); }
        }

        private class FakeSequenceGenerator : ISequenceGenerator
        {
            private readonly IEnumerator<uint> ids = Sequence.Cast<uint>().GetEnumerator();

            public uint Next()
            {
                ids.MoveNext();
                return ids.Current;
            }
        }

        [Test]
        public void Should_consume_ordered_messages()
        {
            var consumer = new GenericMultipleArrivalsConsumer<MyMessage>(Sequence.Length);
            Bus.AddInstanceSubscription(consumer);

            foreach (var u in Sequence)
                Bus.Publish(new MyMessage {Value = (int) u});

            Assert.IsTrue(consumer.WaitForDelivery());
            Assert.AreEqual(Sequence.OrderBy(s => s).ToArray(), consumer.Received.Select(r => (uint)r.Value).ToArray());
        }
    }
}