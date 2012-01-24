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
        private readonly Func<QueueBuilder, string> queueName;
        private readonly IDictionary arguments;

        public DefaultQueueFactory(bool durable,
                                   bool exclusive,
                                   bool autoDelete,
                                   TimeSpan? queueExpiry,
                                   TimeSpan? messageTtl,
                                   Func<QueueBuilder, string> queueName)
        {
            this.durable = durable;
            this.exclusive = exclusive;
            this.autoDelete = autoDelete;
            this.queueName = queueName;
            arguments = new Hashtable();

            if (queueExpiry.HasValue)
                arguments["x-expires"] = (int)queueExpiry.Value.TotalMilliseconds;

            if (messageTtl.HasValue)
                arguments["x-message-ttl"] = (int)messageTtl.Value.TotalMilliseconds;
        }

        public QueueDeclareOk Create(IModel model)
        {
            return model.QueueDeclare(queueName(new QueueBuilder()), durable, exclusive, autoDelete, arguments);
        }
    }
}