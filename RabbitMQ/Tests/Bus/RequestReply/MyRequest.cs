using ProtoBuf;
using Rabbus;

namespace Tests.Bus.RequestReply
{
    [RabbusMessage("RequestExchange")]
    [ProtoContract]
    public class MyRequest
    {
    }
}