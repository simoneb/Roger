using System;
using MbUnit.Framework;
using NSubstitute;
using RabbitMQ.Client;
using Rabbus;
using Rabbus.Internal.Impl;
using Rabbus.Utilities;
using System.Linq;

namespace Tests.Unit
{
    [TestFixture]
    public class DeduplicationFilterTest
    {
        private DeduplicationFilter sut;
        private ICache<RabbusGuid> cache;

        [SetUp]
        public void Setup()
        {
            cache = Substitute.For<ICache<RabbusGuid>>();
            sut = new DeduplicationFilter(cache);
        }

        [Test]
        public void Should_ack_filtered_messages()
        {
            cache.TryAdd(RabbusGuid.Empty).ReturnsForAnyArgs(false);

            var model = Substitute.For<IModel>();
            sut.Filter(new[] {new CurrentMessageInformation {DeliveryTag = 1}}, model).ToArray();

            model.Received().BasicAck(1, false);
        }

        [Test]
        public void Should_not_ack_unfiltered_messages()
        {
            cache.TryAdd(RabbusGuid.Empty).ReturnsForAnyArgs(true);

            var model = Substitute.For<IModel>();
            sut.Filter(new[] { new CurrentMessageInformation { DeliveryTag = 1 } }, model).ToArray();

            model.DidNotReceive().BasicAck(1, false);
        }
    }
}