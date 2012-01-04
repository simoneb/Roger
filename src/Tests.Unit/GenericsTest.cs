using System.IO;
using System.Reflection;
using MbUnit.Framework;
using ProtoBuf;
using Tests.Unit.SupportClasses;

namespace Tests.Unit
{
    [TestFixture]
    public class GenericsTest
    {
        [Test]
        public void Invoking_generic_method()
        {
            var consumer = new MyConsumer();

            consumer.GetType().InvokeMember("Consume", BindingFlags.InvokeMethod, null, consumer, new[]{new MyMessage()});

            Assert.IsTrue(consumer.Consumed);
        }

        [Test]
        public void Invoking_generic_static_method()
        {
            var message = new MyMessage();

            using (var s = new MemoryStream())
            {
                Serializer.Serialize(s, message);

                var method = typeof (Serializer).GetMethod("Deserialize").MakeGenericMethod(typeof (MyMessage));

                var result = method.Invoke(null, new[] {s});

                Assert.IsInstanceOfType<MyMessage>(result);
            }
        }
    }
}