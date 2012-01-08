using System;
using System.Collections.Generic;

namespace Roger
{
    /// <summary>
    /// Resolves no consumers, thus preventing any automatic message subscriptions
    /// </summary>
    public class EmptyConsumerContainer : IConsumerContainer
    {
        public IEnumerable<IConsumer> Resolve(Type consumerType)
        {
            yield break;
        }

        public void Release(IEnumerable<IConsumer> consumers)
        {}

        public IEnumerable<Type> GetAllConsumerTypes()
        {
            yield break;
        }
    }
}