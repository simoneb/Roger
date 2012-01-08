using System;
using System.Collections.Generic;
using System.Linq;

namespace Roger
{
    public class SimpleConsumerContainer : IConsumerContainer
    {
        private readonly IList<IConsumer> consumers = new List<IConsumer>();

        public IEnumerable<IConsumer> Resolve(Type consumerType)
        {
            return consumers.Where(consumerType.IsInstanceOfType);
        }

        public void Release(IEnumerable<IConsumer> consumers)
        {
            // nothing to do here
        }

        public IEnumerable<Type> GetAllConsumerTypes()
        {
            return consumers.Select(c => c.GetType());
        }

        public void Register(IConsumer consumer)
        {
            consumers.Add(consumer);
        }
    }
}