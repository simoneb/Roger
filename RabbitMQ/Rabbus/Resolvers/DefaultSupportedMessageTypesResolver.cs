using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbus.Resolvers
{
    public class DefaultSupportedMessageTypesResolver : ISupportedMessageTypesResolver
    {
        public ISet<Type> Get(Type consumerType)
        {
            return new HashSet<Type>(from i in consumerType.GetInterfaces()
                                     where i.IsGenericType
                                     where typeof (IConsumer<>).IsAssignableFrom(i.GetGenericTypeDefinition())
                                     select i.GetGenericArguments().Single());
        }
    }
}