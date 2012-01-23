using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;

namespace Roger
{
    public class MessageFilterCollection : IMessageFilter
    {
        private readonly LinkedList<IMessageFilter> inner;

        public MessageFilterCollection(params IMessageFilter[] messageFilters)
        {
            inner = new LinkedList<IMessageFilter>(messageFilters);
        }

        IEnumerable<CurrentMessageInformation> IMessageFilter.Filter(IEnumerable<CurrentMessageInformation> input, IModel model)
        {
            return inner.Aggregate(input, (messages, filter) => filter.Filter(messages, model));
        }

        public void AddFirst(IMessageFilter filter)
        {
            inner.AddFirst(filter);
        }

        public void Add(params IMessageFilter[] filters)
        {
            foreach (var filter in filters)
                inner.AddLast(filter);
        }
    }
}