using System;
using MbUnit.Framework;
using NSubstitute;
using Rabbus;
using Rabbus.Reflection;
using System.Linq;
using Tests.Unit.SupportClasses;

namespace Tests.Unit
{
    [TestFixture]
    public class DefaultReflectionTests
    {
        private Exception exception;
        private DefaultReflection sut;
        private HybridExpliticAndBaseConsumer<ConcreteDerivedOfAbstractBase, AbstractBase> hybridExpliticAndBaseConsumer;

        [SetUp]
        public void Setup()
        {
            sut = new DefaultReflection();

            hybridExpliticAndBaseConsumer = new HybridExpliticAndBaseConsumer<ConcreteDerivedOfAbstractBase, AbstractBase>();
            exception = Assert.Throws<InvalidOperationException>(() => sut.InvokeConsume(new MyThrowingConsumer(), new MyMessage()));
        }

        [Test]
        public void Should_throw_original_exception()
        {
            Assert.AreEqual("sorry :(", exception.Message);
        }

        [Test]
        public void Should_preserve_stacktrace()
        {
            Assert.Contains(exception.StackTrace.Split(new[] {Environment.NewLine},
                                                       StringSplitOptions.RemoveEmptyEntries).First(),
                            "line 10");            
        }

        [Test]
        public void Should_invoke_consume_contravariantly()
        {
            var consumer = Substitute.For<Consumer<AbstractBase>.SubclassesInSameAssembly>();

            var message = new ConcreteDerivedOfAbstractBase();
            sut.InvokeConsume(consumer, message);

            consumer.Received().Consume(message);
        }

        [Test]
        public void Should_explicit_derived_consume_on_hybrid_consumer_when_explcit_subclass_matches_received_message()
        {
            var explicitMessage = new ConcreteDerivedOfAbstractBase();

            sut.InvokeConsume(hybridExpliticAndBaseConsumer, explicitMessage);

            Assert.IsTrue(hybridExpliticAndBaseConsumer.DerivedReceived);
            Assert.IsFalse(hybridExpliticAndBaseConsumer.BaseReceived);
        }

        [Test]
        public void Should_invoke_base_class_consume_on_hybrid_consumer_when_explcit_subclass_does_not_match_received_message()
        {
            var nonExplicitMessage = new ConcreteDerivedOfAbstractBase2();

            sut.InvokeConsume(hybridExpliticAndBaseConsumer, nonExplicitMessage);

            Assert.IsFalse(hybridExpliticAndBaseConsumer.DerivedReceived);
            Assert.IsTrue(hybridExpliticAndBaseConsumer.BaseReceived);
        }
    }
}