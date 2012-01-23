using MbUnit.Framework;
using Roger.Internal.Impl;
using Tests.Unit.SupportClasses;

namespace Tests.Unit
{
    [TestFixture]
    public class Message_type_serialization
    {
        [Test]
        public void Should_include_type_and_assembly_name_without_useless_stuff()
        {
            Assert.AreEqual("Tests.Unit.SupportClasses.MyMessage, Tests.Unit", new DefaultTypeResolver().Unresolve(typeof(MyMessage)));
        }
    }
}