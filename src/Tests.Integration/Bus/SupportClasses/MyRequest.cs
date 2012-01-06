using ProtoBuf;
using Roger;

namespace Tests.Integration.Bus.SupportClasses
{
    [RogerMessage("RequestExchange")]
    [ProtoContract]
    public class MyRequest
    {
    }
}