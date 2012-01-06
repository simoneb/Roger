using System;
using System.Collections;

namespace Roger
{
    /// <summary>
    /// Contains contextual information about the message currently being handled by the bus
    /// </summary>
    public class CurrentMessageInformation
    {
        public RogerGuid MessageId { get; set; }
        public object Body { get; set; }
        public RogerEndpoint Endpoint { get; set; }
        public RogerGuid CorrelationId { get; set; }
        public Type MessageType { get; set; }
        public ulong DeliveryTag { get; set; }
        public string Exchange { get; set; }
        public Hashtable Headers { get; set; }
    }
}