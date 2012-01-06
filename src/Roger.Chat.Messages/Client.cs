using System.Runtime.Serialization;

namespace Roger.Chat.Messages
{
    [DataContract]
    public class Client
    {
        [DataMember(Order = 1)]
        public string Endpoint { get; set; }

        [DataMember(Order = 2)]
        public string Username { get; set; }
    }
}