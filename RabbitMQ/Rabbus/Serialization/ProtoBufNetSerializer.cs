using System;
using System.IO;
using ProtoBuf;

namespace Rabbus.Serialization
{
    public class ProtoBufNetSerializer : IMessageSerializer
    {
        public object Deserialize(Type messageType, byte[] body)
        {
            using (var s = new MemoryStream(body))
                return Serializer.NonGeneric.Deserialize(messageType, s);
        }

        public byte[] Serialize(object instance)
        {
            using(var s = new MemoryStream())
            {
                Serializer.NonGeneric.Serialize(s, instance);
                return s.ToArray();
            }
        }
    }
}