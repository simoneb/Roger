using ProtoBuf;
using Roger;

namespace Tests.Integration.Bus.SupportClasses
{
    [ProtoContract]
    [RogerReply(typeof(MyRequest))]
    public class MyReply
    {
    }

    [ProtoContract]
    [RogerMessage("whatever")]
    public class MyWrongReply
    {
    }
}