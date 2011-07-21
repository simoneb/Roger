using ProtoBuf;
using Rabbus;

namespace Tests.Integration.Bus.RequestReply
{
    [RabbusMessage("RequestExchange")]
    [ProtoContract]
    public class MyResponse
    {
    }
}