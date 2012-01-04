using MbUnit.Framework;

namespace Resbit.Test
{
    [TestFixture]
    public class Channels : ResbitTest
    {
        [Test]
        public void Not_null()
        {
            Assert.IsNotNull(Client.Channels());
        }

        [Test]
        public void Not_empty()
        {
            Assert.GreaterThan(Client.Channels().Length, 0);
        }

        [Test]
        public void First_node_name()
        {
            Assert.IsNotNull(Client.Channels()[0].node);
        }

        [TestFixture]
        public class Channel : ResbitTest
        {
            [Test]
            public void Not_null()
            {
                Assert.IsNotNull(Client.Channel(Client.Channels()[0].name));
            }
        }
    }
}