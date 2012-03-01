using System;
using MbUnit.Framework;
using Roger.Internal.Impl;
using Tests.Unit.SupportClasses;

namespace Tests.Unit
{
    [TestFixture]
    public class Message_type_resolution
    {
        private DefaultMessageTypeResolver sut;

        [SetUp]
        public void Setup()
        {
            sut = new DefaultMessageTypeResolver();
        }

        [Test]
        public void Should_include_type_and_assembly_name_without_useless_stuff()
        {
            Assert.AreEqual("Tests.Unit.SupportClasses.MyMessage, Tests.Unit", sut.Unresolve(typeof(MyMessage)));
        }

        [Test]
        public void Should_resolve_existing_type()
        {
            Type type;
            Assert.IsTrue(sut.TryResolve("Tests.Unit.SupportClasses.MyMessage, Tests.Unit", out type));
            Assert.AreEqual(typeof(MyMessage), type);
        }

        [Test]
        public void Should_not_break_if_type_from_name_cannot_be_resolved()
        {
            Type _;
            Assert.IsFalse(sut.TryResolve("Tests.Unit.SupportClasses.MyUnexistingMessage, Tests.Unit", out _));
        }
    }
}