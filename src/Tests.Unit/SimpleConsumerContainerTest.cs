using MbUnit.Framework;
using Roger;
using Tests.Unit.SupportClasses;

namespace Tests.Unit
{
    public class SimpleConsumerContainerTest
    {
        private SimpleConsumerContainer sut;

        [SetUp]
        public void Setup()
        {
            sut = new SimpleConsumerContainer();
        }

        [Test]
        public void Should_resolve_simple_consumer()
        {
            var consumer = new MyMessageConsumer();
            sut.Register(consumer);

            Assert.Contains(sut.Resolve(typeof(MyMessage)), consumer);
        }

        [Test]
        public void Should_resolve_multiple_consumer()
        {
            var consumer = new MultipleMessageConsumer();
            sut.Register(consumer);

            Assert.Contains(sut.Resolve(typeof(MyMessage)), consumer);
            Assert.Contains(sut.Resolve(typeof(MyOtherMessage)), consumer);
        }

        [Test]
        public void Should_resolve_multiple_generic_consumer()
        {
            var consumer = new GenericMultipleMessageConsumer<MyMessage, MyOtherMessage>();
            sut.Register(consumer);

            Assert.Contains(sut.Resolve(typeof(MyMessage)), consumer);
            Assert.Contains(sut.Resolve(typeof(MyOtherMessage)), consumer);
        }


        class MyMessageConsumer : IConsumer<MyMessage>
        {
            public void Consume(MyMessage message)
            {
            }
        }

        class GenericMultipleMessageConsumer<T1, T2> : IConsumer<T1>, IConsumer1<T2>
            where T1 : class
            where T2 : class
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

    public class MultipleMessageConsumer : IConsumer<MyMessage>, IConsumer1<MyOtherMessage>
    {
        public void Consume(MyMessage message)
        {
        }

        public void Consume(MyOtherMessage message)
        {
        }
    }
}