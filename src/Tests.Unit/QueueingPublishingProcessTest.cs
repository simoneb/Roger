using System;
using System.Threading;
using MbUnit.Framework;
using NSubstitute;
using RabbitMQ.Client;
using Roger;
using Roger.Internal;
using Roger.Internal.Impl;
using Roger.Messages;
using Tests.Unit.SupportClasses;

namespace Tests.Unit
{
    [TestFixture]
    public class QueueingPublishingProcessTest
    {
        private IReliableConnection connection;
        private QueueingPublishingProcess sut;
        private IModelWithConnection model;
        private IPublishModule publishModule;
        private Aggregator aggregator;

        [SetUp]
        public void Setup()
        {
            connection = Substitute.For<IReliableConnection>();
            model = Substitute.For<IModelWithConnection>();
            connection.CreateModel().Returns(model);
            publishModule = Substitute.For<IPublishModule>();

            aggregator = new Aggregator();
            sut = new QueueingPublishingProcess(Substitute.For<IIdGenerator>(),
                                                Substitute.For<ISequenceGenerator>(),
                                                Substitute.For<IExchangeResolver>(),
                                                Substitute.For<IMessageSerializer>(),
                                                Substitute.For<IMessageTypeResolver>(),
                                                publishModule, aggregator);
            sut.Start();
        }

        [Test]
        public void Should_not_create_model_until_endpoint_is_known()
        {
            connection.DidNotReceive().CreateModel();
        }

        [Test]
        public void Should_not_publish_messages_until_endpoint_is_known()
        {
            sut.Publish(new MyMessage(), false, false);

            Thread.Sleep(100);

            model.DidNotReceiveWithAnyArgs().BasicPublish(null, null, null, null);
        }

        [Test]
        public void Should_publish_messages_when_endpoint_is_known()
        {
            aggregator.Notify(new ConsumingEnabled(new RogerEndpoint("whataver"), connection));

            sut.Publish(new MyMessage(), false, false);

            Thread.Sleep(100);

            model.ReceivedWithAnyArgs().BasicPublish(null, null, null, null);
        }

        [Test]
        public void Should_notify_modules_when_endpoint_is_known()
        {
            aggregator.Notify(new ConsumingEnabled(new RogerEndpoint("whataver"), connection));
            publishModule.Received().BeforePublishEnabled(model);
        }

        [Test]
        public void Should_notify_module_before_each_publish()
        {
            aggregator.Notify(new ConsumingEnabled(new RogerEndpoint("whatever"), connection));

            sut.Publish(new MyMessage(), false, false);

            Thread.Sleep(100);

            publishModule.Received().BeforePublish(Arg.Any<IDelivery>(), model, Arg.Any<IBasicProperties>(), Arg.Any<Action<BasicReturn>>());
        }

        [Test]
        public void Should_throw_when_replying_out_of_context_of_a_request()
        {
            Assert.Throws<InvalidOperationException>(() => sut.Reply(new MyReply(), null, null, true, true));
        }

        [Test]
        public void Should_enqueue_reply_when_correlation_id_same_as_request_id()
        {
            aggregator.Notify(new ConsumingEnabled(new RogerEndpoint("whatever"), connection));

            var requestId = RogerGuid.NewGuid();

            sut.Reply(new MyReply(), new CurrentMessageInformation{MessageId = requestId, Endpoint = new RogerEndpoint("someEndpoint")}, null, true, true);

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