using MbUnit.Framework;
using Rabbus.RoutingKeys;

namespace Tests.Unit
{
    [TestFixture]
    public class Routing_key_generation
    {
        private readonly DefaultRoutingKeyGenerator sut = new DefaultRoutingKeyGenerator();

        [Test]
        public void Normal_type()
        {
            Assert.AreEqual("Tests.Unit.MyMessage", sut.Generate(typeof(MyMessage)));
        }
    }
}