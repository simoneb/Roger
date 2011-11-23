using System.Runtime.Serialization;

namespace Rabbus.Chat.Messages
{
    [DataContract]
    public class ClientConnected
    {
        [DataMember(Order = 1)]
        public string Username { get; set; }
    }
}