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

        public IEnumerable<IConsumer> Resolve(Type messageRoot)
        {
            return from consumer in consumers
                   from @interface in consumer.GetType().GetInterfaces()
                   where @interface.IsGenericType
                   where typeof (IConsumer).IsAssignableFrom(@interface.GetGenericTypeDefinition())
                   where @interface.GetGenericArguments().Any(a => a == messageRoot)
                   select consumer;
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