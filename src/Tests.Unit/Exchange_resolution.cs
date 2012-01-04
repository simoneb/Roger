using System;
using MbUnit.Framework;
using Rabbus.Internal;
using Rabbus.Internal.Impl;
using Tests.Unit.SupportClasses;

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
        public void Should_return_request_message_exchange_in_case_of_reply()
        {
            Assert.AreEqual("RequestExchange", sut.Resolve(typeof(MyReply)));
        }

        [Test]
        public void Should_not_support_invalid_exchange_names()
        {
            Assert.Throws<ArgumentException>(() => sut.Resolve(typeof(DecoratedWithEmptyString)));
            Assert.Throws<ArgumentException>(() => sut.Resolve(typeof(DecoratedWithNullString)));
            Assert.Throws<ArgumentException>(() => sut.Resolve(typeof(DecoratedWithBlankSpaces)));
            Assert.Throws<ArgumentException>(() => sut.Resolve(typeof(DecoratedWithStringContainingBlankSpaces)));
        }

        [Test]
        public void Should_support_attribute_derived_from_native_one()
        {
            Assert.AreEqual("SomeExchange", sut.Resolve(typeof(DecoratedWithInheritedAttribute)));
        }

        [Test]
        public void Should_not_support_multiple_attributes()
        {
            Assert.Throws<InvalidOperationException>(() => sut.Resolve(typeof(DecoratedWithMultipleAttributes)));
        }

        [Test]
        public void Should_support_attribute_decorating_base_message_class()
        {
            Assert.AreEqual("whatever", sut.Resolve(typeof(InheritorOfDecoratedMessage)));            
        }

        [Test]
        public void Should_not_support_inheritors_decorated_whose_base_is_decorated()
        {
            var e = Assert.Throws<InvalidOperationException>(() => sut.Resolve(typeof (DecoratedWithMultipleAttributes)));
            Assert.AreEqual(ErrorMessages.MultipleRabbusMessageAttributes(typeof(DecoratedWithMultipleAttributes)), e.Message);
        }
    }
}