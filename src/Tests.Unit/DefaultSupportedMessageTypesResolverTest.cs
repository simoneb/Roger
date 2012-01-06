using System;
using MbUnit.Framework;
using Roger;
using Roger.Internal;
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

        class SimpleConsumerOfAbstractClass : IConsumer<AbstractBase>
        {
            public void Consume(AbstractBase message) { }
        }

        class ExplicitMultipleConsumer : IConsumer<MyMessage>, IConsumer<MyOtherMessage>
        {
            public void Consume(MyMessage message) { }
            public void Consume(MyOtherMessage message) { }
        }

        class BaseClassConsumer : Consumer<AbstractBase>.SubclassesInSameAssembly
        {
            public void Consume(AbstractBase message) { }
        }

        class ConsumerOfBaseWithAbstractDerived : Consumer<BaseOfAbstractDerived>.SubclassesInSameAssembly
        {
            public void Consume(BaseOfAbstractDerived message) {}
        }

        class ConsumerOfNonAbstractBase : Consumer<ConcreteBase>.SubclassesInSameAssembly
        {
            public void Consume(ConcreteBase message) { }
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
            Assert.AreElementsEqualIgnoringOrder(new[] { typeof(MyMessage), typeof(MyOtherMessage) }, sut.Resolve(typeof(ExplicitMultipleConsumer)));
        }

        [Test]
        public void Should_throw_if_simple_consumer_of_abstract_class()
        {
            var exception = Assert.Throws<InvalidOperationException>(() => sut.Resolve(typeof (SimpleConsumerOfAbstractClass)));
            Assert.AreEqual(ErrorMessages.NormalConsumerOfAbstractClass(typeof(SimpleConsumerOfAbstractClass), typeof(AbstractBase)), exception.Message);
        }

        [Test]
        public void Consumer_of_base_message_class()
        {
            Assert.AreElementsEqualIgnoringOrder(new[] { typeof(ConcreteDerivedOfAbstractBase), typeof(ConcreteDerivedOfAbstractBase2) }, sut.Resolve(typeof(BaseClassConsumer)));            
        }

        [Test]
        public void Should_throw_for_consumer_of_base_class_which_has_another_abstract_in_the_hierarchy()
        {
            var exception = Assert.Throws<InvalidOperationException>(() => sut.Resolve(typeof(ConsumerOfBaseWithAbstractDerived)));
            Assert.AreEqual(ErrorMessages.SubclassConsumerOfAbstractClassInHierarchy(typeof(ConsumerOfBaseWithAbstractDerived), typeof(AbstractDerived)), exception.Message);
        }

        [Test]
        public void Should_throw_if_consumer_of_base_specifies_non_abstract_message()
        {
            var exception = Assert.Throws<InvalidOperationException>(() => sut.Resolve(typeof(ConsumerOfNonAbstractBase)));
            Assert.AreEqual(ErrorMessages.SubclassConsumerOfNonAbstractClass(typeof(ConsumerOfNonAbstractBase), typeof(ConcreteBase)), exception.Message);
        }
    }
}