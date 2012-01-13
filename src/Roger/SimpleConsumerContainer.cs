using System;
using System.Collections.Generic;
using System.Linq;

namespace Roger
{
    /// <summary>
    /// Simple container in which consumers are registered manually
    /// </summary>
    public class SimpleConsumerContainer : IConsumerContainer
    {
        private readonly IList<IConsumer> consumers;

        public SimpleConsumerContainer(params IConsumer[] consumers)
        {
            this.consumers = new List<IConsumer>(consumers);
        }

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

        public SimpleConsumerContainer Register(IConsumer consumer)
        {
            consumers.Add(consumer);
            return this;
        }
    }
}