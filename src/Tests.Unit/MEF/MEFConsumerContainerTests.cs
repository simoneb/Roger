using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using MbUnit.Framework;
using Roger;
using Tests.Unit.SupportClasses;
using System.Linq;

namespace Tests.Unit.MEF
{
    [TestFixture]
    public class MEFConsumerContainerTests
    {
        private CompositionContainer container;
        private MEFConsumerContainer sut;

        [SetUp]
        public void Setup()
        {
            container = new CompositionContainer(new TypeCatalog(typeof(ExportedAsItself), 
                                                                 typeof(ExportedAsClosedGenericInterface),
                                                                 typeof(ExportedAsOpenGenericInterface),
                                                                 typeof(ExportedAsMultipleGenericInterface),
                                                                 typeof(ExportedAsMultipleGenericAdditionalInterface)));
            sut = new MEFConsumerContainer(container);
        }

        [Test]
        public void Consumers_exported_as_themselves_are_not_returned()
        {
            Assert.DoesNotContain(sut.GetAllConsumerTypes(), typeof(ExportedAsItself));
        }

        [Test]
        [Ignore("Apparently .NET 4.5 beta broke this and it is now supported, which is good, but still perplexing")]
        public void Consumers_exported_as_open_generic_are_not_returned()
        {
            Assert.DoesNotContain(sut.GetAllConsumerTypes(), typeof(ExportedAsOpenGenericInterface));
        }

        [Test]
        public void Consumers_exported_as_closed_generic_interface_are_returned()
        {
            Assert.Contains(sut.GetAllConsumerTypes(), typeof(ExportedAsClosedGenericInterface));
        }

        [Test]
        public void Should_resolve_consumer_exported_as_generic_interface()
        {
            Assert.IsInstanceOfType<ExportedAsClosedGenericInterface>(sut.Resolve(typeof(MyMessage)).Single());
        }

        [Test]
        public void Consumers_exported_as_multiple_generic_are_returned()
        {
            Assert.Contains(sut.GetAllConsumerTypes(), typeof(ExportedAsMultipleGenericInterface));
        }

        [Test]
        public void Should_resolve_consumer_exported_as_multiple_generic_interface()
        {
            Assert.IsInstanceOfType<ExportedAsMultipleGenericInterface>(sut.Resolve(typeof(MyMessageA)).Single());            
            Assert.IsInstanceOfType<ExportedAsMultipleGenericInterface>(sut.Resolve(typeof(MyOtherMessageA)).Single());            
        }

        [Test]
        public void Consumers_exported_as_multiple_additional_generic_are_returned()
        {
            Assert.Contains(sut.GetAllConsumerTypes(), typeof(ExportedAsMultipleGenericAdditionalInterface));
        }

        [Test]
        public void Should_resolve_consumer_exported_as_multiple_additional_generic_interface()
        {
            Assert.IsInstanceOfType<ExportedAsMultipleGenericAdditionalInterface>(sut.Resolve(typeof(MyMessageB)).Single());
            Assert.IsInstanceOfType<ExportedAsMultipleGenericAdditionalInterface>(sut.Resolve(typeof(MyOtherMessageB)).Single());
        }
    }

    [Export]
    class ExportedAsItself : IConsumer<MyMessage>
    {
        public void Consume(MyMessage message)
        {}
    }

    [Export(typeof(IConsumer<MyMessage>))]
    class ExportedAsClosedGenericInterface : IConsumer<MyMessage>
    {
        public void Consume(MyMessage message)
        { }
    }

    [Export(typeof(IConsumer<>))]
    class ExportedAsOpenGenericInterface : IConsumer<MyMessage>
    {
        public void Consume(MyMessage message)
        { }
    }

    [Export(typeof(IConsumer<MyMessageA>))]
    [Export(typeof(IConsumer<MyOtherMessageA>))]
    class ExportedAsMultipleGenericInterface : IConsumer<MyMessageA>, IConsumer<MyOtherMessageA>
    {
        public void Consume(MyMessageA message)
        { }

        public void Consume(MyOtherMessageA message)
        {
        }
    }

    [Export(typeof(IConsumer1<MyMessageB>))]
    [Export(typeof(IConsumer2<MyOtherMessageB>))]
    class ExportedAsMultipleGenericAdditionalInterface : IConsumer1<MyMessageB>, IConsumer2<MyOtherMessageB>
    {
        public void Consume(MyMessageB message)
        { }

        public void Consume(MyOtherMessageB message)
        {
        }
    }

    internal class MyOtherMessageA
    {
    }

    internal class MyMessageA
    {
    }
    internal class MyOtherMessageB
    {
    }

    internal class MyMessageB
    {
    }
}