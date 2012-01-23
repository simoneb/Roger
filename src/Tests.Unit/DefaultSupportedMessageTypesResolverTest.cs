using System;
using MbUnit.Framework;
using Roger;
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

        class MultipleInterfaceConsumer : IConsumer<MyMessage>, IConsumer<MyOtherMessage>
        {
            public void Consume(MyMessage message) { }
            public void Consume(MyOtherMessage message) { }
        }

        class BothBaseAndDerivedConsumer : IConsumer<MyMessage>, IConsumer<MyDerivedMessage>
        {
            public void Consume(MyMessage message) { }
            public void Consume(MyDerivedMessage message) { }
        }

        class DerivedClassOnlyConsumer : IConsumer<MyDerivedMessage>
        {
            public void Consume(MyDerivedMessage message)
            {}
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
            Assert.AreElementsEqualIgnoringOrder(new[] { typeof(MyMessage), typeof(MyOtherMessage) }, sut.Resolve(typeof(MultipleInterfaceConsumer)));
        }

        [Test]
        public void Consumer_consuming_both_base_and_derived_class_should_resolve_to_base_class_only()
        {
            Assert.AreElementsEqualIgnoringOrder(new[] { typeof(MyMessage) }, sut.Resolve(typeof(BothBaseAndDerivedConsumer)));
        }

        [Test]
        public void Consumer_of_derived_class_only_is_not_supported()
        {
            Assert.Throws<InvalidOperationException>(() => sut.Resolve(typeof (DerivedClassOnlyConsumer)));
        }
    }
}