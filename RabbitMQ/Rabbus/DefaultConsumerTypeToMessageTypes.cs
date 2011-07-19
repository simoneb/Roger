using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbus
{
    public class DefaultConsumerTypeToMessageTypes : IConsumerTypeToMessageTypes
    {
        public IEnumerable<Type> Get(Type consumerType)
        {
            return from i in consumerType.GetInterfaces()
                   where i.IsGenericType
                   where typeof(IConsumer<>).IsAssignableFrom(i.GetGenericTypeDefinition())
                   select i.GetGenericArguments().Single();
        }
    }
}