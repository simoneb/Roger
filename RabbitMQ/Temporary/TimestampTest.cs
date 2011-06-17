using MbUnit.Framework;
using RabbitMQ.Client;

namespace Temporary
{
    [TestFixture]
    public class TimestampTest
    {
        [Test]
        public void Default_value()
        {
            Assert.AreEqual(0, new AmqpTimestamp().UnixTime);
        }
    }
}