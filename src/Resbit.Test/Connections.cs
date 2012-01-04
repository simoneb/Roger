using MbUnit.Framework;
using RabbitMQ.Client;

namespace Resbit.Test
{
    [TestFixture]
    public class Connections : ResbitTest
    {
        private IConnection[] additionalConnections;

        [FixtureSetUp]
        public void FixtureSetup()
        {
            var factory = new ConnectionFactory();
            additionalConnections = new[] {factory.CreateConnection(), factory.CreateConnection()};
        }

        [FixtureTearDown]
        public void FixtureTeardown()
        {
            foreach (var connection in additionalConnections)
                connection.Dispose();
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

                Assert.LessThan(Client.Connections().Length, previousConnections);
            }
        }
    }
}