using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class MessageFilterCollection : IMessageFilter
    {
        private readonly LinkedList<IMessageFilter> inner;

        public MessageFilterCollection(params IMessageFilter[] messageFilters)
        {
            inner = new LinkedList<IMessageFilter>(messageFilters);
        }

        public IEnumerable<CurrentMessageInformation> Filter(IEnumerable<CurrentMessageInformation> input, IModel model)
        {
            return inner.Aggregate(input, (messages, filter) => filter.Filter(messages, model));
        }

        public void AddFirst(IMessageFilter filter)
        {
            inner.AddFirst(filter);
        }
    }
}