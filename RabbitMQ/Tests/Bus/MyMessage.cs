using ProtoBuf;
using Rabbus;

namespace Tests.Integration.Bus
{
    [RabbusMessage("TestExchange")]
    [ProtoContract]
    public class MyMessage
    {
        [ProtoMember(1)]
        public int Value { get; set; }
    }
}