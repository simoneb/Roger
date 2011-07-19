using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbus
{
    public class DefaultConsumerToMessageTypes : IConsumerToMessageTypes
    {
        public IEnumerable<Type> Get(IConsumer consumer)
        {
            return from i in consumer.GetType().GetInterfaces()
                   where i.IsGenericType
                   where typeof(IConsumer<>).IsAssignableFrom(i.GetGenericTypeDefinition())
                   select i.GetGenericArguments().Single();
        }
    }
}