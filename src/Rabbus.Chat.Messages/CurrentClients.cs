using System.Runtime.Serialization;

namespace Rabbus.Chat.Messages
{
    [DataContract]
    public class CurrentClients
    {
        [DataMember(Order = 1)]
        public Client[] Clients { get; set; }
    }
}