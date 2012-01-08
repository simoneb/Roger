using System.Runtime.Serialization;

namespace Roger.Chat.Messages
{
    [DataContract]
    public class InstantMessage : ChatMessage
    {
        [DataMember(Order = 1)]
        public string Username { get; set; }

        [DataMember(Order = 2)]
        public string Contents { get; set; }
    }
}