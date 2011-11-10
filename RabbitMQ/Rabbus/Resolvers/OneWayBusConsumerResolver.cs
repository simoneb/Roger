using System;
using System.Collections.Generic;

namespace Rabbus.Resolvers
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

        public IEnumerable<Type> GetAllConsumersTypes()
        {
            yield break;
        }
    }
}