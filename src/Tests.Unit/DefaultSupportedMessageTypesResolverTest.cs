using System;
using MbUnit.Framework;
using Roger;
using Roger.Internal;
using Roger.Internal.Impl;
using Tests.Unit.SupportClasses;

namespace Tests.Unit
{
    [TestFixture]
    public class DefaultSupportedMessageTypesResolverTest
    {
        private DefaultSupportedMessageTypesResolver sut;

        class SimpleConsumer : IConsumer<MyMessage>
        {
            public void Consume(MyMessage message) { }
        }

        class MultipleConsumer : IConsumer<MyMessage>, IConsumer<MyOtherMessage>
        {
            public void Consume(MyMessage message) { }
            public void Consume(MyOtherMessage message) { }
        }

        class BothBaseAndDerivedConsumer : IConsumer<MyMessage>, IConsumer<MyDerivedMessage>
        {
            public void Consume(MyMessage message) { }
            public void Consume(MyDerivedMessage message) { }
        }

        [SetUp]
        public void Setup()
        {
            sut = new DefaultSupportedMessageTypesResolver();
        }

        [Test]
        public void Simple_consumer()
        {
            Assert.AreElementsEqualIgnoringOrder(new[]{typeof(MyMessage)}, sut.Resolve(typeof(SimpleConsumer)));
        }

        [Test]
        public void Consumer_implementing_multiple_interfaces()
        {
            Assert.AreElementsEqualIgnoringOrder(new[] { typeof(MyMessage), typeof(MyOtherMessage) }, sut.Resolve(typeof(MultipleConsumer)));
        }

        [Test]
        public void Consumer_consuming_both_base_and_derived_class()
        {
            Assert.AreElementsEqualIgnoringOrder(new[] { typeof(MyMessage), typeof(MyDerivedMessage) }, sut.Resolve(typeof(BothBaseAndDerivedConsumer)));
        }
    }
}