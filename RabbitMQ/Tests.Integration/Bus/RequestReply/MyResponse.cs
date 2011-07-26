using ProtoBuf;
using Rabbus;
using Rabbus.Utilities;

namespace Tests.Integration.Bus.RequestReply
{
    [ProtoContract]
    [RabbusReply]
    public class MyResponse
    {
    }
}