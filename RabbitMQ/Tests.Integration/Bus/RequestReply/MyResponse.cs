using ProtoBuf;
using Rabbus;

namespace Tests.Integration.Bus.RequestReply
{
    [ProtoContract]
    [RabbusReply]
    public class MyResponse
    {
    }
}