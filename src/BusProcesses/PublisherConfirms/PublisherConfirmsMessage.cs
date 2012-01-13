using ProtoBuf;
using Roger;

namespace BusProcesses.PublisherConfirms
{
    [ProtoContract]
    [RogerMessage("PublisherConfirms")]
    public class PublisherConfirmsMessage
    {
        [ProtoMember(1)]
        public int Counter { get; set; }
    }
}