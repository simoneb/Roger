using ProtoBuf;
using Rabbus;

namespace Tests.Integration.Bus.SupportClasses
{
    [ProtoContract]
    [RabbusMessage("TestExchange")]
    public class MyMessage
    {
        [ProtoMember(1)]
        public int Value { get; set; }
    }

    [ProtoContract]
    [ProtoInclude(10, typeof(MyDerivedMessage))]
    [RabbusMessage("TestExchange")]
    public abstract class MyBaseMessage
    {
        [ProtoMember(1)]
        public int Value { get; set; }
    }

    public class MyDerivedMessage : MyBaseMessage
    {
    }
}