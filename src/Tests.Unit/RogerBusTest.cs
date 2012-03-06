using MbUnit.Framework;
using Roger;

namespace Tests.Unit
{
    [TestFixture]
    public class RogerBusTest
    {
        [Test]
        public void Should_not_throw_if_disposed_before_even_being_started()
        {
            new RogerBus(new DefaultConnectionFactory("whathever")).Dispose();
        }
    }
}