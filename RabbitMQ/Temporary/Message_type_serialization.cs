using MbUnit.Framework;
using Rabbus;

namespace Temporary
{
    [TestFixture]
    public class Message_type_serialization
    {
        [Test]
        public void Should_include_type_and_assembly_name_without_useless_stuff()
        {
            Assert.AreEqual("Temporary.MyMessage, Temporary", new TypeNameGenerator().Generate<MyMessage>());
        }
    }
}