using System;
using MbUnit.Framework;
using Roger.Utilities;

namespace Tests.Unit
{
    public class ConcurrentSelfExpiringCacheTest
    {
        private ConcurrentSelfExpiringCache<int> sut;

        [SetUp]
        public void Setup()
        {
            sut = new ConcurrentSelfExpiringCache<int>(TimeSpan.FromMinutes(1), 1);
        }

        [TearDown]
        public void Teardown()
        {
            SystemTime.Reset();
        }

        [Test]
        public void Should_add_one_item()
        {
            ShouldAdd(1);
        }

        [Test]
        public void Should_not_add_duplicate_items()
        {
            sut.TryAdd(1);
            ShouldNotAdd(1);
        }

        private void ShouldNotAdd(int i)
        {
            Assert.IsFalse(sut.TryAdd(i));
        }

        [Test]
        public void Should_expire_items()
        {
            sut.TryAdd(1);
            SystemTime.GoForward(TimeSpan.FromMinutes(1.1));

            ShouldAdd(1);
        }

        [Test]
        public void Should_expire_items_even_if_no_new_items_are_added()
        {
            sut.TryAdd(1);
            SystemTime.GoForward(TimeSpan.FromMinutes(1.1));
            ShouldAdd(1);
        }

        private void ShouldAdd(int i)
        {
            Assert.IsTrue(sut.TryAdd(i));
        }

        [Test]
        public void Should_slide_expiry_if_same_item_is_added_before_expiry()
        {
            sut.TryAdd(1);
            SystemTime.GoForward(TimeSpan.FromSeconds(50));
            ShouldNotAdd(1);
            SystemTime.GoForward(TimeSpan.FromSeconds(50));
            ShouldNotAdd(1);
        }

        [Test]
        public void Should_not_expire_entries_if_paused()
        {
            sut.TryAdd(1);
            sut.PauseEvictions();
            SystemTime.GoForward(TimeSpan.FromMinutes(1.1));

            ShouldNotAdd(1);
        }

        [Test]
        public void Should_expire_entries_when_resumed_only_after_initial_expiry_has_elapsed_totally_again()
        {
            sut.TryAdd(1);
            sut.PauseEvictions();
            SystemTime.GoForward(TimeSpan.FromMinutes(5));

            sut.ResumeEvictions();

            SystemTime.GoForward(TimeSpan.FromMinutes(1.1));

            ShouldAdd(1);
        }

        [Test]
        public void Should_slide_expiry_of_existing_items_when_resumed()
        {
            sut.TryAdd(1);
            sut.PauseEvictions();
            SystemTime.GoForward(TimeSpan.FromMinutes(1.1));

            sut.ResumeEvictions();
            ShouldNotAdd(1);
        }
    }
}