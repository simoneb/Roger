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

        [SetUp]
        public void Setup()
        {
            sut = new PublisherConfirmsModule();
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

            sut.ProcessUnconfirmed();

            publishingProcess.ReceivedWithAnyArgs(1).Process(null);
        }

        [Test]
        public void Should_not_republish_messages_for_which_ack_or_nack_has_been_received()
        {
            sut.ConnectionEstablished(model);
            model.NextPublishSeqNo.Returns(1ul);

            sut.BeforePublish(Substitute.For<IDeliveryCommand>(), model, Substitute.For<IBasicProperties>(), null);

            model.BasicAcks += Raise.Event<BasicAckEventHandler>(model, new BasicAckEventArgs { DeliveryTag = 1, Multiple = false });

            sut.ProcessUnconfirmed();

            publishingProcess.ReceivedWithAnyArgs(0).Process(null);
        }

        [Test]
        public void Should_not_republish_messages_for_which_ack_has_been_received_after_initial_failure()
        {
            sut.ConnectionEstablished(model);
            model.NextPublishSeqNo.Returns(1ul, 2ul, 3ul);

            sut.BeforePublish(Substitute.For<IDeliveryCommand>(), model, Substitute.For<IBasicProperties>(), null);

            sut.ProcessUnconfirmed();

            model.BasicAcks += Raise.Event<BasicAckEventHandler>(model, new BasicAckEventArgs { DeliveryTag = 2, Multiple = false });

            sut.ProcessUnconfirmed();

            publishingProcess.ReceivedWithAnyArgs(1).Process(null);
        }
    }
}