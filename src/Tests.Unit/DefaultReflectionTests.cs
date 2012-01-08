using System;
using MbUnit.Framework;
using NSubstitute;
using System.Linq;
using Roger;
using Roger.Internal.Impl;
using Tests.Unit.SupportClasses;

namespace Tests.Unit
{
    [TestFixture]
    public class DefaultReflectionTests
    {
        private Exception exception;
        private DefaultReflection sut;

        [SetUp]
        public void Setup()
        {
            sut = new DefaultReflection();

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
            var consumer = Substitute.For<IConsumer<BaseClass>>();

            var message = new DerivedClass();
            sut.InvokeConsume(consumer, message);

            consumer.Received().Consume(message);
        }
    }
}