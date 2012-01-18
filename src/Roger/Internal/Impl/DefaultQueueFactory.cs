using System;
using System.Collections;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class DefaultQueueFactory : IQueueFactory
    {
        private readonly bool durable;
        private readonly bool exclusive;
        private readonly bool autoDelete;
        private readonly IDictionary arguments;

        public DefaultQueueFactory(bool durable = true, bool exclusive = false, bool autoDelete = false, TimeSpan? queueExpiry = null, TimeSpan? messageTtl = null)
        {
            this.durable = durable;
            this.exclusive = exclusive;
            this.autoDelete = autoDelete;
            arguments = new Hashtable();

            if (queueExpiry.HasValue)
                arguments["x-expires"] = (int)queueExpiry.Value.TotalMilliseconds;

            if (messageTtl.HasValue)
                arguments["x-message-ttl"] = (int)messageTtl.Value.TotalMilliseconds;
        }

        public QueueDeclareOk Create(IModel model, string name = "")
        {
            return model.QueueDeclare(name, durable, exclusive, autoDelete, arguments);
        }
    }
}