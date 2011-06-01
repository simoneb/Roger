using System;
using MbUnit.Framework;
using RabbitMQ.Client;

namespace Temporary
{
    [TestFixture]
    public class TimestampTest
    {
        [Test]
        public void TEST_NAME()
        {
            Assert.AreEqual(0, new AmqpTimestamp().UnixTime);
        }
    }
}