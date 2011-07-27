using ProtoBuf;
using Rabbus;

namespace Tests.Integration.Bus.SupportClasses
{
    [RabbusMessage("RequestExchange")]
    [ProtoContract]
    public class MyRequest
    {
    }
}