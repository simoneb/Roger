using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            get { yield return new DeduplicationFilter(Bus, TimeSpan.FromSeconds(5), 1); }
        }

        protected override IIdGenerator IdGenerator
        {
            get { return new StaticIdGenerator(); }
        }

        private class StaticIdGenerator : IIdGenerator
        {
            private RogerGuid? guid;

            public RogerGuid Next()
            {
                return (RogerGuid) (guid ?? (guid = RogerGuid.NewGuid()));
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

        [Test]
        public void Will_receive_duplicate_messages_if_delivered_after_cache_expiry()
        {
            var consumer = new GenericMultipleArrivalsConsumer<MyMessage>(20);
            Bus.AddInstanceSubscription(consumer);

            for (int i = 0; i < 10; i++)
                Bus.Publish(new MyMessage { Value = i });

            Assert.IsFalse(consumer.WaitForDelivery());

            Thread.Sleep(TimeSpan.FromSeconds(6));

            for (int i = 0; i < 10; i++)
                Bus.Publish(new MyMessage { Value = i });

            Assert.IsFalse(consumer.WaitForDelivery());
            
            Assert.AreEqual(2, consumer.Received.Count);
            Assert.AreEqual(0, consumer.Received.Select(r => r.Value).Distinct().Single());
        }
    }
}