using MbUnit.Framework;
using NSubstitute;
using RabbitMQ.Client;
using System.Linq;
using Roger;
using Roger.Internal.Impl;
using Roger.Utilities;

namespace Tests.Unit
{
    [TestFixture]
    public class DeduplicationFilterTest
    {
        private DeduplicationFilter sut;
        private ICache<RogerGuid> cache;

        [SetUp]
        public void Setup()
        {
            cache = Substitute.For<ICache<RogerGuid>>();
            sut = new DeduplicationFilter(Substitute.For<IRabbitBus>(), cache);
        }

        [Test]
        public void Should_ack_filtered_messages()
        {
            cache.TryAdd(RogerGuid.Empty).ReturnsForAnyArgs(false);

            var model = Substitute.For<IModel>();
            sut.Filter(new[] {new CurrentMessageInformation {DeliveryTag = 1}}, model).ToArray();

            model.Received().BasicAck(1, false);
        }

        [Test]
        public void Should_not_ack_unfiltered_messages()
        {
            cache.TryAdd(RogerGuid.Empty).ReturnsForAnyArgs(true);

            var model = Substitute.For<IModel>();
            sut.Filter(new[] { new CurrentMessageInformation { DeliveryTag = 1 } }, model).ToArray();

            model.DidNotReceive().BasicAck(1, false);
        }
    }
}