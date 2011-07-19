using System;

namespace Rabbus
{
    public class CurrentMessageInformation
    {
        public string ReplyTo { get; set; }
        public string CorrelationId { get; set; }
        public Type MessageType { get; set; }
    }
}