using System;
using System.Collections.Generic;
using System.Linq;
using Rabbus.Errors;

namespace Rabbus.Resolvers
{
    public class DefaultSupportedMessageTypesResolver : ISupportedMessageTypesResolver
    {
        public ISet<Type> Resolve(Type consumerType)
        {
            var simpleConsumerMessages = new HashSet<Type>(from i in consumerType.GetInterfaces()
                                                           where i.IsGenericType
                                                           where !typeof(Consumer<>.AndDerivedInSameAssembly).IsAssignableFrom(i.GetGenericTypeDefinition())
                                                           where typeof (IConsumer<>).IsAssignableFrom(i.GetGenericTypeDefinition())
                                                           select i.GetGenericArguments().Single());

            foreach (var message in simpleConsumerMessages.Where(m => m.IsAbstract))
                throw new InvalidOperationException(ErrorMessages.NormalConsumerOfAbstractClass(consumerType, message));

            return simpleConsumerMessages;
        }
    }
}