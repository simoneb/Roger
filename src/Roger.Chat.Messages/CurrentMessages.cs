using System.Runtime.Serialization;

namespace Roger.Chat.Messages
{
    [DataContract]
    public class CurrentMessages : ChatMessage
    {
        [DataMember(Order = 1)]
        public InstantMessage[] Messages { get; set; }
    }
}