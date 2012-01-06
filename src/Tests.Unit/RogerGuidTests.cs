using System;
using MbUnit.Framework;
using Roger;

namespace Tests.Unit
{
    [TestFixture]
    public class RogerGuidTests
    {
        [Test]
        public void When_instantiated_with_no_arguments_should_be_empty()
        {
            Assert.AreEqual(new RogerGuid(), new RogerGuid(Guid.Empty));
        }

        [Test]
        public void Should_be_equal_when_two_instances_have_same_plain_guid()
        {
            var guid = Guid.NewGuid();

            Assert.AreEqual(new RogerGuid(guid), new RogerGuid(guid));
        }

        [Test]
        public void Should_throw_if_instantiating_with_invalid_string()
        {
            Assert.Throws<ArgumentNullException>(() => new RogerGuid(null));
            Assert.Throws<FormatException>(() => new RogerGuid(""));
        }

        [Test]
        public void Should_create_new_random_guid()
        {
            Assert.AreNotEqual(RogerGuid.NewGuid(), new RogerGuid());
        }

        [Test]
        public void Empty_should_be_equal_to_newly_instantiated_value()
        {
            Assert.AreEqual(new RogerGuid(), RogerGuid.Empty);
        }
    }
}