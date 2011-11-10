using System;

namespace Rabbus
{
    public class CurrentMessageInformation
    {
        //public Guid MessageId { get; set; }
        public object Body { get; set; }
        public RabbusEndpoint Endpoint { get; set; }
        public Guid CorrelationId { get; set; }
        public Type MessageType { get; set; }
        public ulong DeliveryTag { get; set; }
        public string Exchange { get; set; }
    }
}