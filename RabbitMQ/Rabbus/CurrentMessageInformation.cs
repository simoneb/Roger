using System;
using Rabbus.GuidGeneration;

namespace Rabbus
{
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