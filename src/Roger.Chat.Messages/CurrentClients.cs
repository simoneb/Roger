using System.Runtime.Serialization;

namespace Roger.Chat.Messages
{
    [DataContract]
    public class CurrentClients : ChatMessage
    {
        [DataMember(Order = 1)]
        public Client[] Clients { get; set; }
    }
}