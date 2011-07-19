using System;

namespace Rabbus.Serialization
{
    public interface IMessageSerializer
    {
        object Deserialize(Type messageType, byte[] body);
        byte[] Serialize(object instance);
    }
}