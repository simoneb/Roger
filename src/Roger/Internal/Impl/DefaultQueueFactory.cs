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

        public DefaultQueueFactory(bool durable = true, bool exclusive = false, bool autoDelete = false, uint queueExpiryMilliseconds = 0, uint messageTtlMilliseconds = 0)
        {
            this.durable = durable;
            this.exclusive = exclusive;
            this.autoDelete = autoDelete;
            arguments = new Hashtable();

            if (queueExpiryMilliseconds > 0)
                arguments["x-expires"] = queueExpiryMilliseconds;

            if (messageTtlMilliseconds > 0)
                arguments["x-message-ttl"] = messageTtlMilliseconds;
        }

        public QueueDeclareOk Create(IModel model)
        {
            return model.QueueDeclare("", durable, exclusive, autoDelete, arguments);
        }
    }
}