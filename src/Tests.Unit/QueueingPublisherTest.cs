using System;
using System.Threading;
using MbUnit.Framework;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Roger;
using Roger.Internal;
using Roger.Internal.Impl;

namespace Tests.Unit
{
    [TestFixture]
    public class QueueingPublisherTest
    {
        private IReliableConnection connection;
        private QueueingPublishingProcess sut;
        private IModel model;

        [SetUp]
        public void Setup()
        {
            connection = Substitute.For<IReliableConnection>();
            model = Substitute.For<IModel>();
            connection.CreateModel().Returns(model);
 
            sut = new QueueingPublishingProcess(connection,
                                                Substitute.For<IIdGenerator>(),
                                                Substitute.For<ISequenceGenerator>(),
                                                Substitute.For<IExchangeResolver>(),
                                                Substitute.For<IMessageSerializer>(),
                                                Substitute.For<ITypeResolver>(), Substitute.For<IRogerLog>(),
                                                Substitute.For<Func<RogerEndpoint>>(), 
                                                TimeSpan.Zero);
            sut.Start();
        }

        [Test]
        public void Should_not_create_model_until_connection_is_established()
        {
            connection.DidNotReceive().CreateModel();
        }

        [Test]
        public void Should_publish_messages_when_connection_is_established()
        {
            connection.ConnectionEstabilished += Raise.Event<Action>();

            sut.Publish(new object());

            Thread.Sleep(100);

            model.ReceivedWithAnyArgs().BasicPublish(null, null, null, null);
        }

        [Test]
        public void Should_republish_messages_for_which_no_ack_or_nack_has_been_received()
        {
            connection.ConnectionEstabilished += Raise.Event<Action>();
            model.NextPublishSeqNo.Returns(1ul, 2ul, 3ul);

            sut.Publish(new object());

            Thread.Sleep(100);

            sut.ProcessUnconfirmed();

            Thread.Sleep(100);

            model.ReceivedWithAnyArgs(2).BasicPublish(null, null, null, null);
        }

        [Test]
        public void Should_not_republish_messages_for_which_ack_or_nack_has_been_received()
        {
            connection.ConnectionEstabilished += Raise.Event<Action>();
            model.NextPublishSeqNo.Returns(1ul);

            sut.Publish(new object());

            Thread.Sleep(100);

            model.BasicAcks += Raise.Event<BasicAckEventHandler>(model, new BasicAckEventArgs {DeliveryTag = 1, Multiple = false});

            sut.ProcessUnconfirmed();

            Thread.Sleep(100);

            model.ReceivedWithAnyArgs(1).BasicPublish(null, null, null, null);
        }

        [Test]
        public void Should_not_republish_messages_for_which_ack_has_been_received_after_initial_failure()
        {
            connection.ConnectionEstabilished += Raise.Event<Action>();
            model.NextPublishSeqNo.Returns(1ul, 2ul, 3ul);

            sut.Publish(new object());

            Thread.Sleep(100);

            sut.ProcessUnconfirmed();

            Thread.Sleep(100);

            model.BasicAcks += Raise.Event<BasicAckEventHandler>(model, new BasicAckEventArgs { DeliveryTag = 2, Multiple = false });

            sut.ProcessUnconfirmed();

            Thread.Sleep(100);

            model.ReceivedWithAnyArgs(2).BasicPublish(null, null, null, null);
        }
    }
}