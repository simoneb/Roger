using System;
using System.Threading;
using MbUnit.Framework;
using NSubstitute;
using RabbitMQ.Client;
using Roger;
using Roger.Internal;
using Roger.Internal.Impl;
using Tests.Unit.SupportClasses;

namespace Tests.Unit
{
    [TestFixture]
    public class QueueingPublishingProcessTest
    {
        private IReliableConnection connection;
        private QueueingPublishingProcess sut;
        private IModel model;
        private IPublishModule publishModule;

        [SetUp]
        public void Setup()
        {
            connection = Substitute.For<IReliableConnection>();
            model = Substitute.For<IModel>();
            connection.CreateModel().Returns(model);
            publishModule = Substitute.For<IPublishModule>();

            sut = new QueueingPublishingProcess(connection,
                                                Substitute.For<IIdGenerator>(),
                                                Substitute.For<ISequenceGenerator>(),
                                                Substitute.For<IExchangeResolver>(),
                                                Substitute.For<IMessageSerializer>(),
                                                Substitute.For<ITypeResolver>(),
                                                Substitute.For<Func<RogerEndpoint>>(),
                                                publishModule);
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

            sut.Publish(new MyMessage(), false);

            Thread.Sleep(100);

            model.ReceivedWithAnyArgs().BasicPublish(null, null, null, null);
        }

        [Test]
        public void Should_invoke_modules_when_connection_established()
        {
            connection.ConnectionEstabilished += Raise.Event<Action>();
            publishModule.Received().BeforePublishEnabled(model);
        }

        [Test]
        public void Should_notify_module_before_each_publish()
        {
            connection.ConnectionEstabilished += Raise.Event<Action>();

            sut.Publish(new MyMessage(), false);

            Thread.Sleep(100);

            publishModule.Received().BeforePublish(Arg.Any<IDelivery>(), model, Arg.Any<IBasicProperties>(), Arg.Any<Action<BasicReturn>>());
        }

        [Test]
        public void Should_not_publish_messages_until_connection_is_established()
        {
            sut.Publish(new MyMessage(), false);

            Thread.Sleep(100);

            model.DidNotReceiveWithAnyArgs().BasicPublish(null, null, null, null);
        }

        [Test]
        public void Should_throw_when_replying_out_of_context_of_a_request()
        {
            Assert.Throws<InvalidOperationException>(() => sut.Reply(new MyReply(), null, null, true));
        }

        [Test]
        public void Should_enqueue_reply_when_correlation_id_same_as_request_id()
        {
            connection.ConnectionEstabilished += Raise.Event<Action>();

            RogerGuid requestId = RogerGuid.NewGuid();

            sut.Reply(new MyReply(), new CurrentMessageInformation{MessageId = requestId, Endpoint = new RogerEndpoint("someEndpoint")}, null, true);

            Thread.Sleep(100);

            model.Received().BasicPublish(Arg.Any<string>(),
                                          Arg.Any<string>(),
                                          true,
                                          false,
                                          Arg.Is<IBasicProperties>(p => p.CorrelationId == requestId), 
                                          Arg.Any<byte[]>());
        }
    }
}