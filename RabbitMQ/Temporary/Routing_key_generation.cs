using MbUnit.Framework;
using Rabbus.RoutingKeys;

namespace Temporary
{
    [TestFixture]
    public class Routing_key_generation
    {
        private readonly DefaultRoutingKeyGenerator sut = new DefaultRoutingKeyGenerator();

        [Test]
        public void Normal_type()
        {
            Assert.AreEqual("Temporary.MyMessage", sut.Generate(typeof (MyMessage)));
        }
    }
}