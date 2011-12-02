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
            var simpleConsumerMessages = (from i in consumerType.GetInterfaces()
                                         where i.IsGenericType
                                         where typeof (IConsumer<>).IsAssignableFrom(i.GetGenericTypeDefinition())
                                         select i.GetGenericArguments().Single()).ToArray();

            foreach (var message in simpleConsumerMessages.Where(m => m.IsAbstract))
                throw new InvalidOperationException(ErrorMessages.NormalConsumerOfAbstractClass(consumerType, message));

            var subclassesConsumerMessages = (from i in consumerType.GetInterfaces()
                                             where i.IsGenericType
                                             where typeof(Consumer<>.SubclassesInSameAssembly).IsAssignableFrom(i.GetGenericTypeDefinition())
                                             let baseClass = i.GetGenericArguments().Single()
                                             from @class in baseClass.Assembly.GetTypes()
                                             where baseClass.IsAssignableFrom(@class) && @class != baseClass
                                             select @class).ToArray();

            foreach (var message in subclassesConsumerMessages.Where(m => m.IsAbstract))
                throw new InvalidOperationException(ErrorMessages.SubclassConsumerOfAbstractClass(consumerType, message));

            return new HashSet<Type>(simpleConsumerMessages.Union(subclassesConsumerMessages));
        }
    }
}