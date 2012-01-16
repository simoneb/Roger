using System;
using System.Collections.Generic;
using System.Linq;
using MbUnit.Framework;
using Roger;
using Roger.Internal.Impl;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public class Duplicate_messages : With_default_bus
    {
        protected override IEnumerable<IMessageFilter> MessageFilters
        {
            get { yield return new ResequencingDeduplicationFilter(); }
        }

        protected override ISequenceGenerator SequenceGenerator
        {
            get { return new ReverseSequenceGenerator(); }
        }

        private class ReverseSequenceGenerator : ISequenceGenerator
        {
            private readonly IEnumerator<uint> ids = new[] {1, 2, 3, 4, 5, 5, 4, 3, 2, 1}.Cast<uint>().GetEnumerator();

            public uint Next(Type messageType)
            {
                ids.MoveNext();
                return ids.Current;
            }
        }

        [Test]
        public void Should_not_consume_duplicate_messages()
        {
            var consumer = new GenericMultipleArrivalsConsumer<MyMessage>(10);
            Bus.AddInstanceSubscription(consumer);

            for (int i = 0; i < 10; i++)
                Bus.Publish(new MyMessage {Value = i});

            Assert.IsFalse(consumer.WaitForDelivery());
            Assert.AreEqual(5, consumer.Received.Count);
            Assert.AreElementsEqual(Enumerable.Range(0, 5), consumer.Received.Select(r => r.Value));
        }
    }
}