using System;
using System.Net;
using MbUnit.Framework;

namespace Resbit.Test
{
    [TestFixture]
    public class Queues : ResbitTest
    {
        private string queueName;

        [FixtureSetUp]
        public void Setup()
        {
            queueName = Guid.NewGuid().ToString();
            Client.PutQueue(queueName);
        }

        [FixtureTearDown]
        public void Teardown()
        {
            Client.DeleteQueue(queueName);
        }

        [Test]
        public void Not_null()
        {
            Assert.IsNotNull(Client.Queues());
        }

        [Test]
        public void Not_empty()
        {
            Assert.GreaterThan(Client.Queues().Length, 0);
        }

        [Test]
        public void First_queue_name()
        {
            Assert.IsNotNull(Client.Queues()[0].name);
        }

        [TestFixture]
        public class VHost : ResbitTest
        {
            [Test]
            public void Not_null()
            {
                Assert.IsNotNull(Client.Queues("/"));
            }

            [Test]
            public void Not_empty()
            {
                Assert.GreaterThan(Client.Queues("/").Length, 0);
            }

            [Test]
            public void First_queue_name()
            {
                Assert.IsNotNull(Client.Queues("/")[0].name);
            }

            [TestFixture]
            public class Queue : ResbitTest
            {
                [Test]
                public void Not_null()
                {
                    Assert.IsNotNull(Client.GetQueue(Client.Queues()[0].name));
                }

                [Test]
                public void First_queue_name()
                {
                    Assert.IsNotNull(Client.GetQueue(Client.Queues()[0].name).name);
                }

                [Test]
                public void Put()
                {
                    var name = Guid.NewGuid().ToString();
                    Client.PutQueue(name);

                    Assert.IsNotNull(Client.GetQueue(name));
                }

                [Test]
                public void Delete()
                {
                    var name = Guid.NewGuid().ToString();
                    Client.PutQueue(name);

                    Client.DeleteQueue(name);

                    Assert.Throws<WebException>(() => Client.GetQueue(name));
                }
            }
        }
    }
}