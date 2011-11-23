using System.Runtime.Serialization;

namespace Rabbus.Chat.Messages
{
    [DataContract]
    public class InstantMessage
    {
        [DataMember(Order = 1)]
        public string Username { get; set; }

        [DataMember(Order = 2)]
        public string Contents { get; set; }
    }
}