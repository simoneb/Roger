using Castle.MicroKernel.Registration;
using Castle.Windsor;
using MbUnit.Framework;
using Roger;
using Roger.Windsor;
using Tests.Unit.SupportClasses;

namespace Tests.Unit
{
    public class WindsorConsumerContainerTest
    {
        private IWindsorContainer container;
        private WindsorConsumerContainer sut;

        [SetUp]
        public void Setup()
        {
            container = new WindsorContainer();
            sut = new WindsorConsumerContainer(container);
        }

        [Test]
        public void Should_resolve_simple_consumer()
        {
            var consumer = new MyMessageConsumer();
            container.Register(Component.For<IConsumer<MyMessage>>().Instance(consumer));

            Assert.Contains(sut.Resolve(typeof(MyMessage)), consumer);
        }

        [Test]
        public void Should_resolve_multiple_consumer()
        {
            var consumer = new MultipleMessageConsumer();
            container.Register(Component.For(typeof(IConsumer<MyMessage>), typeof(IConsumer1<MyOtherMessage>)).Instance(consumer));

            Assert.Contains(sut.Resolve(typeof(MyMessage)), consumer);
            Assert.Contains(sut.Resolve(typeof(MyOtherMessage)), consumer);
        }

        [Test]
        public void Should_resolve_multiple_generic_consumer()
        {
            var consumer = new GenericMultipleMessageConsumer<MyMessage, MyOtherMessage>();
            container.Register(Component.For(typeof(IConsumer<MyMessage>), typeof(IConsumer1<MyOtherMessage>)).Instance(consumer));

            Assert.Contains(sut.Resolve(typeof(MyMessage)), consumer);
            Assert.Contains(sut.Resolve(typeof(MyOtherMessage)), consumer);
        }

        class MyMessageConsumer : IConsumer<MyMessage>
        {
            public void Consume(MyMessage message)
            {
            }
        }

        class GenericMultipleMessageConsumer<T1, T2> : IConsumer<T1>, IConsumer1<T2> where T1 : class where T2 : class
        {
            public void Consume(T1 message)
            {
                
            }

            public void Consume(T2 message)
            {
                
            }
        }

        class MultipleMessageConsumer : IConsumer<MyMessage>, IConsumer1<MyOtherMessage>
        {
            public void Consume(MyMessage message)
            {
            }

            public void Consume(MyOtherMessage message)
            {
            }
        }
    }
}