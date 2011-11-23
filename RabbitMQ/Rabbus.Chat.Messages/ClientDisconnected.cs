using System.Runtime.Serialization;

namespace Rabbus.Chat.Messages
{
    [DataContract]
    public class ClientDisconnected
    {
        [DataMember(Order = 1)]
        public string Endpoint { get; set; }
    }
}