using MbUnit.Framework;

namespace Resbit.Test
{
    [TestFixture]
    public class Overview : ResbitTest
    {
        [Test]
        public void Not_null()
        {
            Assert.IsNotNull(Client.Overview());
        }

        [Test]
        public void Management_version()
        {
            Assert.IsNotNull(Client.Overview().management_version);
        }
    }
}