using MbUnit.Framework;
using RabbitMQ.Client;

namespace Resbit.Test
{
    [TestFixture]
    public class Connections : ResbitTest
    {
        private IConnection additionalConnection;

        [FixtureSetUp]
        public void FixtureSetup()
        {
            additionalConnection = new ConnectionFactory().CreateConnection();
        }

        [FixtureTearDown]
        public void FixtureTeardown()
        {
            additionalConnection.Dispose();
        }

        [Test]
        public void Not_null()
        {
            Assert.IsNotNull(Client.Connections());
        }

        [Test]
        public void Not_empty()
        {
            Assert.GreaterThan(Client.Connections().Length, 0);
        }

        [Test]
        public void First_node_name()
        {
            Assert.IsNotNull(Client.Connections()[0].node);
        }

        [TestFixture]
        public class Connection : ResbitTest
        {
            [Test]
            public void Not_null()
            {
                Assert.IsNotNull(Client.GetConnection(Client.Connections()[0].name));
            }

            [Test]
            public void Delete()
            {
                var connections = Client.Connections();
                var previousConnections = connections.Length;

                Client.DeleteConnection(connections[0].name);

                Assert.AreEqual(previousConnections - 1, Client.Connections().Length);
            }
        }
    }
}