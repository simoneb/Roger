using System;
using MbUnit.Framework;
using Rabbus.Reflection;
using System.Linq;
using Tests.Unit.SupportClasses;

namespace Tests.Unit
{
    [TestFixture]
    public class DefaultReflectionTests
    {
        private Exception exception;

        [SetUp]
        public void Setup()
        {
            var sut = new DefaultReflection();

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
    }
}