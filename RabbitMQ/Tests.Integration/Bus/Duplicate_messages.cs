using System;
using System.Collections.Generic;
using System.Linq;
using MbUnit.Framework;
using Rabbus;
using Rabbus.Internal.Impl;
using Tests.Integration.Bus.SupportClasses;

namespace Tests.Integration.Bus
{
    public class Duplicate_messages : With_default_bus
    {
        protected override IEnumerable<IMessageFilter> MessageFilters
        {
            get { yield return new DeduplicationFilter(TimeSpan.FromSeconds(10)); }
        }

        protected override IIdGenerator IdGenerator
        {
            get { return new FakeIdGenerator(); }
        }

        private class FakeIdGenerator : IIdGenerator
        {
            private RabbusGuid? guid;

            public RabbusGuid Next()
            {
                return (RabbusGuid) (guid ?? (guid = RabbusGuid.NewGuid()));
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
            Assert.AreEqual(1, consumer.Received.Count);
            Assert.AreEqual(0, consumer.Received.Single().Value);
        }
    }
}