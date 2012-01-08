using System;
using System.Collections.Generic;
using System.Linq;

namespace Roger.Internal.Impl
{
    internal class DefaultSupportedMessageTypesResolver : ISupportedMessageTypesResolver
    {
        public ISet<Type> Resolve(Type consumerType)
        {
            var explicitMessages = from i in consumerType.GetInterfaces()
                                   where i.IsGenericType
                                   where typeof (IConsumer<>).IsAssignableFrom(i.GetGenericTypeDefinition())
                                   select i.GetGenericArguments().Single();

            return new HashSet<Type>(explicitMessages);
        }
    }
}