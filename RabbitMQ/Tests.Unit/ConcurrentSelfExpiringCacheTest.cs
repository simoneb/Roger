using System;
using MbUnit.Framework;
using Rabbus.Utilities;

namespace Tests.Unit
{
    public class ConcurrentSelfExpiringCacheTest
    {
        private ConcurrentSelfExpiringCache<int> sut;

        [SetUp]
        public void Setup()
        {
            sut = new ConcurrentSelfExpiringCache<int>(TimeSpan.FromMinutes(1));
        }

        [TearDown]
        public void Teardown()
        {
            SystemTime.Reset();
        }

        [Test]
        public void Should_add_one_item()
        {
            Assert.IsTrue(sut.TryAdd(1));
        }

        [Test]
        public void Should_not_add_duplicate_items()
        {
            sut.TryAdd(1);
            Assert.IsFalse(sut.TryAdd(1));
        }

        [Test]
        public void Should_expire_items_when_after_new_items()
        {
            sut.TryAdd(1);
            SystemTime.GoForward(TimeSpan.FromMinutes(1.1));
            sut.TryAdd(2);

            Assert.IsTrue(sut.TryAdd(1));
        }

        [Test]
        public void Should_not_expire_items_if_no_new_items_are_added()
        {
            sut.TryAdd(1);
            SystemTime.GoForward(TimeSpan.FromMinutes(1.1));
            Assert.IsFalse(sut.TryAdd(1));
        }

        [Test]
        public void Should_pospone_expiry_if_same_item_is_added_before_expiry()
        {
            sut.TryAdd(1);
            SystemTime.GoForward(TimeSpan.FromSeconds(50));
            sut.TryAdd(2);
            Assert.IsFalse(sut.TryAdd(1));
            SystemTime.GoForward(TimeSpan.FromSeconds(50));
            sut.TryAdd(3);
            Assert.IsFalse(sut.TryAdd(1));
        }
    }
}