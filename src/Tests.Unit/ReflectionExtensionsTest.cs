using System;
using System.Linq;
using MbUnit.Framework;
using NSubstitute;
using Roger;
using Tests.Unit.SupportClasses;
using Roger.Internal.Impl;

namespace Tests.Unit
{
    [TestFixture]
    public class ReflectionExtensionsTest
    {
        private InvalidOperationException exception;

        [SetUp]
        public void Setup()
        {
            exception = Assert.Throws<InvalidOperationException>(() => new MyThrowingConsumer().InvokePreservingStackTrace(new MyMessage()));
        }

        [Test]
        public void Should_return_hierarchy_root_of_hierarchy()
        {
            Assert.AreEqual(typeof (BaseClass), typeof (DerivedClass).HierarchyRoot());
        }

        [Test]
        public void Should_return_hierarchy_ordered_by_most_derived_first()
        {
            Assert.AreElementsEqual(new[]{typeof(DerivedClass), typeof(BaseClass)}, typeof(DerivedClass).Hierarchy());
        }

        [Test]
        public void Hierarchy_of_non_derived_class_should_be_the_class_itself()
        {
            Assert.AreElementsEqual(new[]{typeof(BaseClass)}, typeof(BaseClass).Hierarchy());
        }

        [Test]
        public void Hierarchy_root_of_non_derived_class_should_be_the_class_itself()
        {
            Assert.AreEqual(typeof(BaseClass), typeof(BaseClass).HierarchyRoot());
        }

        [Test]
        public void Should_throw_original_exception()
        {
            Assert.AreEqual("sorry :(", exception.Message);
        }

        [Test]
        public void Should_preserve_stacktrace()
        {
            Assert.Contains(exception.StackTrace.Split(new[] { Environment.NewLine },
                                                       StringSplitOptions.RemoveEmptyEntries).First(),
                            "line 10");
        }

        [Test]
        public void Should_invoke_consume_contravariantly()
        {
            var consumer = Substitute.For<IConsumer<BaseClass>>();

            var message = new DerivedClass();
            consumer.InvokePreservingStackTrace(message);

            consumer.Received().Consume(message);
        }
    }
}