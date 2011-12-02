using System;
using MbUnit.Framework;
using Rabbus;
using Rabbus.Errors;
using Rabbus.Resolvers;
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

        class SimpleConsumerOfAbstractClass : IConsumer<MyBaseMessage>
        {
            public void Consume(MyBaseMessage message) { }
        }

        class ExplicitMultipleConsumer : IConsumer<MyMessage>, IConsumer<MyOtherMessage>
        {
            public void Consume(MyMessage message) { }
            public void Consume(MyOtherMessage message) { }
        }

        class BaseClassConsumer : Consumer<MyBaseMessage>.AndDerivedInSameAssembly
        {
            public void Consume(MyBaseMessage message) { }
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
            Assert.AreEqual(ErrorMessages.NormalConsumerOfAbstractClass(typeof(SimpleConsumerOfAbstractClass), typeof(MyBaseMessage)), exception.Message);
        }

        [Test]
        public void Consumer_of_base_message_class()
        {
            Assert.AreElementsEqualIgnoringOrder(new[] { typeof(MyDerivedMessage), typeof(MyOtherDerivedMessage) }, sut.Resolve(typeof(BaseClassConsumer)));            
        }
    }
}