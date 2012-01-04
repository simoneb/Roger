using System;

namespace Rabbus
{
    public interface IMessageSerializer
    {
        object Deserialize(Type messageType, byte[] body);
        byte[] Serialize(object instance);
        string ContentType { get; }
    }
}