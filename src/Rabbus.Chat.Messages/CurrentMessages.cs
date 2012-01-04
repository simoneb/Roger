using System.Runtime.Serialization;

namespace Rabbus.Chat.Messages
{
    [DataContract]
    public class CurrentMessages
    {
        [DataMember(Order = 1)]
        public InstantMessage[] Messages { get; set; }
    }
}