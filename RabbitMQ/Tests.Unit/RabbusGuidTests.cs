using System;
using MbUnit.Framework;
using Rabbus.GuidGeneration;

namespace Tests.Unit
{
    [TestFixture]
    public class RabbusGuidTests
    {
        [Test]
        public void When_instantiated_with_no_arguments_should_be_empty()
        {
            Assert.AreEqual(new RabbusGuid(), new RabbusGuid(Guid.Empty));
        }

        [Test]
        public void Should_be_equal_when_two_instances_have_same_plain_guid()
        {
            var guid = Guid.NewGuid();

            Assert.AreEqual(new RabbusGuid(guid), new RabbusGuid(guid));
        }

        [Test]
        public void Should_throw_if_instantiating_with_invalid_string()
        {
            Assert.Throws<ArgumentNullException>(() => new RabbusGuid(null));
            Assert.Throws<FormatException>(() => new RabbusGuid(""));
        }

        [Test]
        public void Should_create_new_random_guid()
        {
            Assert.AreNotEqual(RabbusGuid.NewGuid(), new RabbusGuid());
        }

        [Test]
        public void Empty_should_be_equal_to_newly_instantiated_value()
        {
            Assert.AreEqual(new RabbusGuid(), RabbusGuid.Empty);
        }
    }
}