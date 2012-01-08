using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using MbUnit.Framework;
using Roger;
using Roger.MEF;
using Tests.Unit.SupportClasses;
using System.Linq;

namespace Tests.Unit.MEF
{
    [TestFixture]
    public class MEFConsumerContainerTests
    {
        private CompositionContainer container;
        private MEFConsumerContainer sut;
        private TypeCatalog typeCatalog;

        [SetUp]
        public void Setup()
        {
            typeCatalog = new TypeCatalog();
            container = new CompositionContainer(new TypeCatalog(typeof(ExportedAsItself), 
                                                                 typeof(ExportedAsGenericInterface),
                                                                 typeof(ExportedAsOpenGenericInterface)));
            sut = new MEFConsumerContainer(container);
        }

        [Test]
        public void Consumers_exported_as_themselves_are_not_returned()
        {
            Assert.DoesNotContain(sut.GetAllConsumerTypes(), typeof(ExportedAsItself));
        }

        [Test]
        public void Consumers_exported_as_IConsumer_generic_interface_are_returned()
        {
            Assert.Contains(sut.GetAllConsumerTypes(), typeof(ExportedAsGenericInterface));
        }

        [Test]
        public void Consumers_exported_as_open_generic_are_not_returned()
        {
            Assert.DoesNotContain(sut.GetAllConsumerTypes(), typeof(ExportedAsOpenGenericInterface));
        }

        [Test]
        public void Should_resolve_consumer_exported_as_generic_interface()
        {
            Assert.IsInstanceOfType<ExportedAsGenericInterface>(sut.Resolve(typeof(IConsumer<MyMessage>)).Single());
        }
    }

    [Export]
    class ExportedAsItself : IConsumer<MyMessage>
    {
        public void Consume(MyMessage message)
        {}
    }

    [Export(typeof(IConsumer<MyMessage>))]
    class ExportedAsGenericInterface : IConsumer<MyMessage>
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
}