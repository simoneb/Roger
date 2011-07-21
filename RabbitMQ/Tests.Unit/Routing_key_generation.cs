using MbUnit.Framework;
using Rabbus.Resolvers;

namespace Tests.Unit
{
    [TestFixture]
    public class Routing_key_generation
    {
        private readonly DefaultRoutingKeyResolver sut = new DefaultRoutingKeyResolver();

        [Test]
        public void Normal_type()
        {
            Assert.AreEqual("Tests.Unit.MyMessage", sut.Resolve(typeof(MyMessage)));
        }
    }
}