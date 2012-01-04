using ProtoBuf;
using Rabbus;

namespace Tests.Integration.Bus.SupportClasses
{
    [ProtoContract]
    [RabbusReply(typeof(MyRequest))]
    public class MyReply
    {
    }

    [ProtoContract]
    [RabbusMessage("whatever")]
    public class MyWrongReply
    {
    }
}