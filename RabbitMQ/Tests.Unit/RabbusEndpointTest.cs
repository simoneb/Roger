using MbUnit.Framework;
using Rabbus;

namespace Tests.Unit
{
    [TestFixture]
    public class RabbusEndpointTest
    {
        [Test]
        public void Should_be_equal_if_queue_is_equal()
        {
            Assert.AreEqual(new RabbusEndpoint("myQueue"), new RabbusEndpoint("myQueue"));
        }

        [Test]
        public void Should_have_same_hashcode_when_equal()
        {
            Assert.AreEqual(new RabbusEndpoint("myQueue").GetHashCode(), new RabbusEndpoint("myQueue").GetHashCode());
        }

        [Test]
        public void Should_not_be_equal_if_queue_is_not()
        {
            Assert.AreNotEqual(new RabbusEndpoint("myQueue"), new RabbusEndpoint("anotherQueue"));            
        }

        [Test]
        public void Should_not_have_same_hashcode_when_not_equal()
        {
            Assert.AreNotEqual(new RabbusEndpoint("myQueue").GetHashCode(), new RabbusEndpoint("anotherQueue").GetHashCode());
        }
    }
}