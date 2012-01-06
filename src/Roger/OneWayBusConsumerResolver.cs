using System;
using System.Collections.Generic;

namespace Roger
{
    /// <summary>
    /// Resolves no consumers, thus preventing any automatic message subscriptions
    /// </summary>
    public class OneWayBusConsumerResolver : IConsumerResolver
    {
        public IEnumerable<IConsumer> Resolve(Type messageType)
        {
            yield break;
        }

        public void Release(IEnumerable<IConsumer> consumers)
        {}

        public ISet<Type> GetAllSupportedMessageTypes()
        {
            return new HashSet<Type>();
        }
    }
}