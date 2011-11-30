using System;
using Rabbus.GuidGeneration;

namespace Rabbus
{
    /// <summary>
    /// Contains contextual information about the message currently being handled by the bus
    /// </summary>
    public class CurrentMessageInformation
    {
        public RabbusGuid MessageId { get; set; }
        public object Body { get; set; }
        public RabbusEndpoint Endpoint { get; set; }
        public RabbusGuid CorrelationId { get; set; }
        public Type MessageType { get; set; }
        public ulong DeliveryTag { get; set; }
        public string Exchange { get; set; }
    }
}