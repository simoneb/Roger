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

        public DefaultQueueFactory(bool durable = false, bool exclusive = true, bool autoDelete = false, IDictionary arguments = null)
        {
            this.durable = durable;
            this.exclusive = exclusive;
            this.autoDelete = autoDelete;
            this.arguments = arguments;
        }

        public QueueDeclareOk Create(IModel model)
        {
            return model.QueueDeclare("", durable, exclusive, autoDelete, arguments);
        }
    }
}