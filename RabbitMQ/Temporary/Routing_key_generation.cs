using MbUnit.Framework;
using Rabbus;

namespace Temporary
{
    [TestFixture]
    public class Routing_key_generation
    {
        private DefaultRoutingKeyGenerationStrategy sut = new DefaultRoutingKeyGenerationStrategy();

        [Test]
        public void Normal_type()
        {
            Assert.AreEqual("Temporary.MyMessage", sut.GetRoutingKey(typeof (MyMessage)));
        }
    }
}