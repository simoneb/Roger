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
        private IScheduler timer;

        [SetUp]
        public void Setup()
        {
            timer = Substitute.For<IScheduler>();
            sut = new PublisherConfirmsModule(timer);
            publishingProcess = Substitute.For<IPublishingProcess>();
            model = Substitute.For<IModel>();

            sut.Initialize(publishingProcess);
        }

        [Test]
        public void Should_republish_messages_for_which_no_ack_or_nack_has_been_received()
        {
            sut.BeforePublishEnabled(model);
            model.NextPublishSeqNo.Returns(1ul, 2ul, 3ul);

            sut.BeforePublish(Substitute.For<IDelivery>(), model, Substitute.For<IBasicProperties>(), null);

            timer.Callback += Raise.Event<Action>();

            publishingProcess.ReceivedWithAnyArgs(1).Process(null);
        }

        [Test]
        public void Should_not_republish_messages_for_which_ack_or_nack_has_been_received()
        {
            sut.BeforePublishEnabled(model);
            model.NextPublishSeqNo.Returns(1ul);

            sut.BeforePublish(Substitute.For<IDelivery>(), model, Substitute.For<IBasicProperties>(), null);

            model.BasicAcks += Raise.Event<BasicAckEventHandler>(model, new BasicAckEventArgs { DeliveryTag = 1, Multiple = false });

            timer.Callback += Raise.Event<Action>();

            publishingProcess.ReceivedWithAnyArgs(0).Process(null);
        }

        [Test]
        public void Should_not_republish_messages_for_which_ack_has_been_received_after_initial_failure()
        {
            sut.BeforePublishEnabled(model);
            model.NextPublishSeqNo.Returns(1ul, 2ul, 3ul);

            sut.BeforePublish(Substitute.For<IDelivery>(), model, Substitute.For<IBasicProperties>(), null);

            timer.Callback += Raise.Event<Action>();

            model.BasicAcks += Raise.Event<BasicAckEventHandler>(model, new BasicAckEventArgs { DeliveryTag = 2, Multiple = false });

            timer.Callback += Raise.Event<Action>();

            publishingProcess.ReceivedWithAnyArgs(1).Process(null);
        }

        [Test]
        public void After_publish_is_disabled_should_process_messages_that_were_unconfirmed_so_as_to_put_them_on_top_of_the_list()
        {
            model.NextPublishSeqNo.Returns(1ul);
            sut.BeforePublish(Substitute.For<IDelivery>(), model, Substitute.For<IBasicProperties>(), null);

            sut.AfterPublishDisabled(model);

            publishingProcess.ReceivedWithAnyArgs(1).Process(null);
        }

        [Test]
        public void Should_ignore_threshold_for_messages_published_after_publishing_has_been_disabled()
        {
            sut = new PublisherConfirmsModule(Substitute.For<IScheduler>(), TimeSpan.FromSeconds(10000 /* a high value */));
            sut.Initialize(publishingProcess);

            model.NextPublishSeqNo.Returns(1ul);
            sut.BeforePublish(Substitute.For<IDelivery>(), model, Substitute.For<IBasicProperties>(), null);

            sut.AfterPublishDisabled(model);

            publishingProcess.ReceivedWithAnyArgs(1).Process(null);
        }
    }
}