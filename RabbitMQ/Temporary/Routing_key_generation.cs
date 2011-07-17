using MbUnit.Framework;
using Rabbus;

namespace Temporary
{
    [TestFixture]
    public class Routing_key_generation
    {
        private DefaultRoutingKeyGenerator sut = new DefaultRoutingKeyGenerator();

        [Test]
        public void Normal_type()
        {
            Assert.AreEqual("Temporary.MyMessage", sut.GetRoutingKey(typeof (MyMessage)));
        }
    }
}