using MbUnit.Framework;
using NSubstitute;
using RabbitMQ.Client;
using Roger;
using Roger.Internal;
using Roger.Internal.Impl;
using Roger.Messages;

namespace Tests.Unit
{
    [TestFixture]
    public class DefaultConsumingProcessTest
    {
        private IReliableConnection connection;
        private IModel model;
        private DefaultConsumingProcess sut;
        private IQueueFactory queueFactory;
        private Aggregator aggregator;

        [SetUp]
        public void Setup()
        {
            connection = Substitute.For<IReliableConnection>();
            model = Substitute.For<IModel>();
            connection.CreateModel().Returns(model);

            queueFactory = Substitute.For<IQueueFactory>();
            aggregator = new Aggregator();

            sut = new DefaultConsumingProcess(Substitute.For<IIdGenerator>(),
                                              Substitute.For<IExchangeResolver>(),
                                              Substitute.For<IMessageSerializer>(), 
                                              Substitute.For<ITypeResolver>(),
                                              Substitute.For<IConsumerContainer>(), 
                                              Substitute.For<IMessageFilter>(),
                                              queueFactory,
                                              Substitute.For<IConsumerInvoker>(),
                                              new RogerOptions(), aggregator);
        }

        [Test]
        public void Should_create_queue_when_connection_is_established_for_the_first_time()
        {
            queueFactory.Create(model).Returns(new QueueDeclareOk("someQueue", 1, 1));

            aggregator.Notify(new ConnectionEstablished(connection));

            queueFactory.Received().Create(model);
        }

        [Test]
        public void Should_not_recreate_queue_upon_following_connections()
        {
            queueFactory.Create(model).Returns(new QueueDeclareOk("someQueue", 1, 1));

            aggregator.Notify(new ConnectionEstablished(connection));
            aggregator.Notify(new ConnectionEstablished(connection));

            queueFactory.Received(1).Create(model);
        }

        [Test]
        public void Should_delete_queue_when_bus_is_disposed_of()
        {
            queueFactory.Create(model).Returns(new QueueDeclareOk("someQueue", 1, 1));
            aggregator.Notify(new ConnectionEstablished(connection));

            sut.Dispose();

            model.Received().QueueDelete("someQueue");
        }
    }
}