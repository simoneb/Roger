using MbUnit.Framework;
using Roger;

namespace Tests.Unit
{
    [TestFixture]
    public class RogerEndpointTest
    {
        [Test]
        public void Should_be_equal_if_queue_is_equal()
        {
            Assert.AreEqual(new RogerEndpoint("myQueue"), new RogerEndpoint("myQueue"));
        }

        [Test]
        public void Should_have_same_hashcode_when_equal()
        {
            Assert.AreEqual(new RogerEndpoint("myQueue").GetHashCode(), new RogerEndpoint("myQueue").GetHashCode());
        }

        [Test]
        public void Should_not_be_equal_if_queue_is_not()
        {
            Assert.AreNotEqual(new RogerEndpoint("myQueue"), new RogerEndpoint("anotherQueue"));            
        }

        [Test]
        public void Should_not_have_same_hashcode_when_not_equal()
        {
            Assert.AreNotEqual(new RogerEndpoint("myQueue").GetHashCode(), new RogerEndpoint("anotherQueue").GetHashCode());
        }
    }
}