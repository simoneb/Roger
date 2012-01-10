using System;
using MbUnit.Framework;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Roger.Internal;
using Roger.Internal.Impl;

namespace Tests.Unit
{
    [TestFixture]
    public class PublisherConfirmsModuleTest
    {
        private PublisherConfirmsModule sut;
        private IPublishingProcess publishingProcess;
        private IModel model;
        private ITimer timer;

        [SetUp]
        public void Setup()
        {
            timer = Substitute.For<ITimer>();
            sut = new PublisherConfirmsModule(timer);
            publishingProcess = Substitute.For<IPublishingProcess>();
            model = Substitute.For<IModel>();

            sut.Initialize(publishingProcess);
        }

        [Test]
        public void Should_republish_messages_for_which_no_ack_or_nack_has_been_received()
        {
            sut.ConnectionEstablished(model);
            model.NextPublishSeqNo.Returns(1ul, 2ul, 3ul);

            sut.BeforePublish(Substitute.For<IDeliveryCommand>(), model, Substitute.For<IBasicProperties>(), null);

            timer.Callback += Raise.Event<Action>();

            publishingProcess.ReceivedWithAnyArgs(1).Process(null);
        }

        [Test]
        public void Should_not_republish_messages_for_which_ack_or_nack_has_been_received()
        {
            sut.ConnectionEstablished(model);
            model.NextPublishSeqNo.Returns(1ul);

            sut.BeforePublish(Substitute.For<IDeliveryCommand>(), model, Substitute.For<IBasicProperties>(), null);

            model.BasicAcks += Raise.Event<BasicAckEventHandler>(model, new BasicAckEventArgs { DeliveryTag = 1, Multiple = false });

            timer.Callback += Raise.Event<Action>();

            publishingProcess.ReceivedWithAnyArgs(0).Process(null);
        }

        [Test]
        public void Should_not_republish_messages_for_which_ack_has_been_received_after_initial_failure()
        {
            sut.ConnectionEstablished(model);
            model.NextPublishSeqNo.Returns(1ul, 2ul, 3ul);

            sut.BeforePublish(Substitute.For<IDeliveryCommand>(), model, Substitute.For<IBasicProperties>(), null);

            timer.Callback += Raise.Event<Action>();

            model.BasicAcks += Raise.Event<BasicAckEventHandler>(model, new BasicAckEventArgs { DeliveryTag = 2, Multiple = false });

            timer.Callback += Raise.Event<Action>();

            publishingProcess.ReceivedWithAnyArgs(1).Process(null);
        }

        [Test]
        public void When_connection_is_established_should_process_messages_that_were_unconfirmed()
        {
            sut.ConnectionEstablished(model);
            model.NextPublishSeqNo.Returns(1ul);

            sut.BeforePublish(Substitute.For<IDeliveryCommand>(), model, Substitute.For<IBasicProperties>(), null);

            sut.ConnectionEstablished(model);

            publishingProcess.ReceivedWithAnyArgs(1).Process(null);
        }

        [Test]
        public void When_connection_is_established_should_process_messages_that_were_unconfirmed_ignoring_threshold()
        {
            sut = new PublisherConfirmsModule(Substitute.For<ITimer>(), TimeSpan.FromSeconds(1000));
            sut.Initialize(publishingProcess);

            sut.ConnectionEstablished(model);
            model.NextPublishSeqNo.Returns(1ul);

            sut.BeforePublish(Substitute.For<IDeliveryCommand>(), model, Substitute.For<IBasicProperties>(), null);

            sut.ConnectionEstablished(model);

            publishingProcess.ReceivedWithAnyArgs(1).Process(null);
        }
    }
}