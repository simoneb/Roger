using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using MbUnit.Framework;
using Roger.Internal.Impl;
using Tests.Unit.SupportClasses;

namespace Tests.Unit
{
    [TestFixture]
    public class Routing_key_generation
    {
        private readonly DefaultRoutingKeyResolver sut = new DefaultRoutingKeyResolver();

        [Test]
        public void Normal_type()
        {
            Assert.AreEqual("Tests.Unit.SupportClasses.MyMessage", sut.Resolve(typeof(MyMessage)));
        }

        [Test]
        public void Derived_type()
        {
            Assert.AreEqual("Tests.Unit.SupportClasses.MyMessage.MyDerivedMessage", sut.Resolve(typeof(MyDerivedMessage)));            
        }
        
        [Test]
        public void Should_support_routing_keys_long_up_to_255_chars()
        {
            var type = CreateLongNamedType(255);

            Assert.DoesNotThrow(() => sut.Resolve(type));
        }

        [Test]
        public void Should_throw_if_routing_key_is_longer_than_255_chars()
        {
            var type = CreateLongNamedType(256);

            Assert.Throws<InvalidOperationException>(() => sut.Resolve(type));
        }

        private static Type CreateLongNamedType(int typeNameLength)
        {
            return AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("temp"), AssemblyBuilderAccess.Run)
                .DefineDynamicModule("temp")
                .DefineType(new string(Enumerable.Repeat('n', typeNameLength/2).ToArray()) + "." + new string(Enumerable.Repeat('a', typeNameLength/2 - 1 + typeNameLength%2).ToArray()))
                .CreateType();
        }
    }
}