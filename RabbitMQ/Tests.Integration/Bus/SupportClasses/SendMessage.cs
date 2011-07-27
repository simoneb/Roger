using ProtoBuf;
using Rabbus;

namespace Tests.Integration.Bus.SupportClasses
{
    [RabbusMessage("SendExchange")]
    [ProtoContract]
    public class SendMessage
    {
    }
}