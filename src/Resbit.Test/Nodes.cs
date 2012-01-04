using MbUnit.Framework;

namespace Resbit.Test
{
    [TestFixture]
    public class Nodes : ResbitTest
    {
        [Test]
        public void Not_null()
        {
            Assert.IsNotNull(Client.Nodes());
        }

        [Test]
        public void Not_empty()
        {
            Assert.GreaterThan(Client.Nodes().Length, 0);
        }

        [Test]
        public void First_node_name()
        {
            Assert.IsNotNull(Client.Nodes()[0].name);
        }
    }
}