using System.Runtime.Serialization;

namespace Roger.Chat.Messages
{
    [DataContract]
    public class ClientConnected : ChatMessage
    {
        [DataMember(Order = 1)]
        public string Username { get; set; }
    }

    public abstract class ChatMessage
    {
    }
}