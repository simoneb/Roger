using System;
using System.Linq;
using MbUnit.Framework;
using NSubstitute;
using RabbitMQ.Client;
using Roger;
using Roger.Internal;
using Roger.Internal.Impl;

namespace Tests.Unit
{
    [TestFixture]
    public class DefaultConsumingProcessTest
    {
        private IReliableConnection connection;
        private IModel model;
        private DefaultConsumingProcess sut;
        private IQueueFactory queueFactory;

        [SetUp]
        public void Setup()
        {
            connection = Substitute.For<IReliableConnection>();
            model = Substitute.For<IModel>();
            connection.CreateModel().Returns(model);

            queueFactory = Substitute.For<IQueueFactory>();
            sut = new DefaultConsumingProcess(connection, 
                                              Substitute.For<IIdGenerator>(),
                                              Substitute.For<IExchangeResolver>(),
                                              Substitute.For<IMessageSerializer>(), 
                                              Substitute.For<ITypeResolver>(),
                                              Substitute.For<IConsumerContainer>(), 
                                              Enumerable.Empty<IMessageFilter>(), 
                                              queueFactory, 
                                              Substitute.For<IConsumerInvoker>(),
                                              false);
        }

        [Test]
        public void Should_create_queue_when_connection_is_established_for_the_first_time()
        {
            queueFactory.Create(model).Returns(new QueueDeclareOk("someQueue", 1, 1));

            connection.ConnectionEstabilished += Raise.Event<Action>();

            queueFactory.Received().Create(model);
        }

        [Test]
        public void Should_not_recreate_queue_upon_following_connections()
        {
            queueFactory.Create(model).Returns(new QueueDeclareOk("someQueue", 1, 1));

            connection.ConnectionEstabilished += Raise.Event<Action>();
            connection.ConnectionEstabilished += Raise.Event<Action>();

            queueFactory.Received(1).Create(model);
        }

        [Test]
        public void Should_delete_queue_when_bus_is_disposed_of()
        {
            queueFactory.Create(model).Returns(new QueueDeclareOk("someQueue", 1, 1));
            connection.ConnectionEstabilished += Raise.Event<Action>();

            sut.Dispose();

            model.Received().QueueDelete("someQueue");
        }
    }
}