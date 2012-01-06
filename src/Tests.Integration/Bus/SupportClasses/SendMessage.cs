using ProtoBuf;
using Roger;

namespace Tests.Integration.Bus.SupportClasses
{
    [RogerMessage("SendExchange")]
    [ProtoContract]
    public class SendMessage
    {
    }
}