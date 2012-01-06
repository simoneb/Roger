using ProtoBuf;
using Roger;

namespace Tests.Integration.Bus.SupportClasses
{
    [ProtoContract]
    [RogerMessage("TestExchange")]
    public class MyMessage
    {
        [ProtoMember(1)]
        public int Value { get; set; }
    }

    [ProtoContract]
    [ProtoInclude(10, typeof(MyDerivedMessage))]
    [RogerMessage("TestExchange")]
    public abstract class MyBaseMessage
    {
        [ProtoMember(1)]
        public int Value { get; set; }
    }

    public class MyDerivedMessage : MyBaseMessage
    {
    }
}