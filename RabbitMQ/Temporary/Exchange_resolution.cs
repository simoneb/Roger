using System;
using MbUnit.Framework;
using Rabbus;
using Rabbus.Exchanges;

namespace Tests.Unit
{
    [TestFixture]
    public class Exchange_resolution
    {
        private DefaultExchangeResolver sut;

        [SetUp]
        public void Setup()
        {
            sut = new DefaultExchangeResolver();
        }

        [Test]
        public void Should_throw_if_message_not_decorated()
        {
            Assert.Throws<InvalidOperationException>(() => sut.Resolve(typeof (NonDecorated)));
        }

        [Test]
        public void Simple_message()
        {
            Assert.AreEqual("SomeExchange", sut.Resolve(typeof(SimplyDecorated)));
        }

        [Test]
        public void Should_not_support_invalid_exchange_names()
        {
            Assert.Throws<ArgumentException>(() => sut.Resolve(typeof(DecoratedWithEmptyString)));
            Assert.Throws<ArgumentException>(() => sut.Resolve(typeof(DecoratedWithNullString)));
            Assert.Throws<ArgumentException>(() => sut.Resolve(typeof(DecoratedWithBlankSpaces)));
            Assert.Throws<ArgumentException>(() => sut.Resolve(typeof(DecoratedWithStringContainingBlankSpaces)));
        }
    }

    [RabbusMessage("hi hi")]
    public class DecoratedWithStringContainingBlankSpaces
    {
    }

    [RabbusMessage(" ")]
    public class DecoratedWithBlankSpaces
    {
    }

    [RabbusMessage(null)]
    public class DecoratedWithNullString
    {
    }

    [RabbusMessage("")]
    public class DecoratedWithEmptyString
    {
    }

    [RabbusMessage("SomeExchange")]
    public class SimplyDecorated
    {
    }

    public class NonDecorated
    {
    }
}