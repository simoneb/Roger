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
            var factory = new DefaultQueueFactory(durable: false, queueExpiry: TimeSpan.FromSeconds(2));

            using (var connection = Helpers.CreateSafeShutdownConnection())
            using (var model = connection.CreateModel())
            {
                var queue = factory.Create(model);

                Assert.AreEqual(queue, RestClient.GetQueue(queue).name);

                Thread.Sleep(3000);

                Assert.Throws<WebException>(() => RestClient.GetQueue(queue));
            }
        }

        [Test]
        public void Should_respect_message_ttl()
        {
            var factory = new DefaultQueueFactory(durable: false, messageTtl: TimeSpan.FromSeconds(1));

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