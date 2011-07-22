using ProtoBuf;
using Rabbus;

namespace Tests.Integration.Bus.RequestReply
{
    [RabbusMessage("ResponseExchange")]
    [ProtoContract]
    public class MyResponse
    {
    }
}