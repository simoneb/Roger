using MbUnit.Framework;

namespace Resbit.Test
{
    [TestFixture]
    public class AllConfiguration : ResbitTest
    {
        private dynamic allConfiguration;

        [SetUp]
        public void Setup()
        {
            allConfiguration = Client.AllConfiguration();            
        }

        [Test]
        public void Not_null()
        {
            Assert.IsNotNull(allConfiguration);
        }

        [Test]
        public void Users()
        {
            Assert.IsNotNull(allConfiguration.users);
        }
    }
}