using System;
using System.Net;
using System.Threading;
using Common;
using MbUnit.Framework;
using Roger.Internal.Impl;

namespace Tests.Integration
{
    [TestFixture]
    public class QueueFactoryTest : With_rabbitmq_broker
    {
        [Test]
        public void Should_respect_queue_lease()
        {
            var factory = new DefaultQueueFactory(false, false, false, TimeSpan.FromSeconds(2), null, _ => "");

            using (var connection = Helpers.CreateSafeShutdownConnection())
            using (var model = connection.CreateModel())
            {
                var queue = factory.Create(model);

                Assert.AreEqual(queue, BrokerHttp.GetQueue(queue).name);

                Thread.Sleep(3000);

                Assert.Throws<WebException>(() => BrokerHttp.GetQueue(queue));
            }
        }

        [Test]
        public void Should_respect_message_ttl()
        {
            var factory = new DefaultQueueFactory(false, false, false, null, TimeSpan.FromSeconds(1), _ => "");

            using (var connection = Helpers.CreateSafeShutdownConnection())
            using (var model = connection.CreateModel())
            {
                var queue = factory.Create(model);
                model.BasicPublish("", queue, null, BitConverter.GetBytes(1));

                Thread.Sleep(1500);

                Assert.IsNull(model.BasicGet(queue, true));
            }
        }
    }
}