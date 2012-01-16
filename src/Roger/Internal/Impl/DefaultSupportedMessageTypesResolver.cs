using System;
using System.Collections.Generic;
using System.Linq;

namespace Roger.Internal.Impl
{
    internal class DefaultSupportedMessageTypesResolver : ISupportedMessageTypesResolver
    {
        public ISet<Type> Resolve(Type consumerType)
        {
            var supportedMessages = (from i in consumerType.GetInterfaces()
                                     where i.IsGenericType
                                     where typeof (IConsumer<>).IsAssignableFrom(i.GetGenericTypeDefinition())
                                     let message = i.GetGenericArguments().Single()
                                     select new {Root = message.HierarchyRoot(), Message = message}).ToArray();


            if (supportedMessages.Any(m => supportedMessages.All(i => i.Message != m.Root) && m.Message != m.Root))
                throw new InvalidOperationException("Consuming only derived class of message hierarchy is not supported");

            return new HashSet<Type>(supportedMessages.Select(m => m.Root));
        }
    }
}