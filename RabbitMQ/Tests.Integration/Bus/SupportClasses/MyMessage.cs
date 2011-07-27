using ProtoBuf;
using Rabbus;

namespace Tests.Integration.Bus.SupportClasses
{
    [RabbusMessage("TestExchange")]
    [ProtoContract]
    public class MyMessage
    {
        [ProtoMember(1)]
        public int Value { get; set; }
    }
}