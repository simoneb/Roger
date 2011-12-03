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
            var explicitMessages = (from i in consumerType.GetInterfaces()
                                    where i.IsGenericType
                                    where typeof (IConsumer<>).IsAssignableFrom(i.GetGenericTypeDefinition())
                                    select i.GetGenericArguments().Single()).ToArray();

            foreach (var message in explicitMessages.Where(m => m.IsAbstract))
                throw new InvalidOperationException(ErrorMessages.NormalConsumerOfAbstractClass(consumerType, message));

            var fromBaseMessages = (from i in consumerType.GetInterfaces()
                                    where i.IsGenericType
                                    where typeof (Consumer<>.SubclassesInSameAssembly).IsAssignableFrom(i.GetGenericTypeDefinition())
                                    let baseClass = i.GetGenericArguments().Single()
                                    from @class in baseClass.Assembly.GetTypes()
                                    where baseClass.IsAssignableFrom(@class) && @class != baseClass
                                    group @class by baseClass into byCommonBase
                                    select byCommonBase).ToArray();

            foreach (var fromBaseGroup in fromBaseMessages)
            {
                if(!fromBaseGroup.Key.IsAbstract)
                    throw new InvalidOperationException(ErrorMessages.SubclassConsumerOfNonAbstractClass(consumerType, fromBaseGroup.Key));

                foreach (var message in fromBaseGroup.Where(m => m.IsAbstract))
                    throw new InvalidOperationException(ErrorMessages.SubclassConsumerOfAbstractClassInHierarchy(consumerType, message));
            }

            return new HashSet<Type>(explicitMessages.Union(fromBaseMessages.SelectMany(types => types)));
        }
    }
}