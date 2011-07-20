using System;

namespace Rabbus
{
    public class CurrentMessageInformation
    {
        public object Body { get; set; }
        public string ReplyTo { get; set; }
        public string CorrelationId { get; set; }
        public Type MessageType { get; set; }
        public ulong DeliveryTag { get; set; }
    }
}