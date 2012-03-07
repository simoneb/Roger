using System.Collections;
using System.Diagnostics;
using System.Threading;
using MbUnit.Framework;
using NSubstitute;
using RabbitMQ.Client;
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

        [Test]
        public void Start_should_be_idempotent_on_an_already_started_bus()
        {
            var connectionFactory = Substitute.For<IConnectionFactory>();
            var connection = Substitute.For<IConnection>();
            connectionFactory.CreateConnection().Returns(connection);
            var model = Substitute.For<IModel>();
            connection.CreateModel().Returns(model);
            model.QueueDeclare("", false, false, false, null).ReturnsForAnyArgs(new QueueDeclareOk("", 0, 0));

            var bus = new RogerBus(connectionFactory);

            bus.Start();
            bus.Start();

            connectionFactory.Received(1).CreateConnection();

            bus.Dispose();
        }
    }
}