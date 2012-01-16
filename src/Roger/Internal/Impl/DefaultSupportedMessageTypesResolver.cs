using System;
using System.Collections.Generic;
using System.Linq;

namespace Roger.Internal.Impl
{
    internal class DefaultSupportedMessageTypesResolver : ISupportedMessageTypesResolver
    {
        private static readonly IEnumerable<Type> ConsumerInterfaces = new[]
        {
            typeof (IConsumer<>),
            typeof (IConsumer<,>),
            typeof (IConsumer<,,>)
        };

        public ISet<Type> Resolve(Type consumerType)
        {
            var supportedMessages = (from i in consumerType.GetInterfaces()
                                     where i.IsGenericType
                                     where IsConsumer(i)
                                     from message in i.GetGenericArguments()
                                     select new {Root = message.HierarchyRoot(), Message = message}).ToArray();


            if (supportedMessages.Any(m => supportedMessages.All(i => i.Message != m.Root) && m.Message != m.Root))
                throw new InvalidOperationException("Consuming derived class of message hierarchy is not supported, you should consume entire hierarchy");

            return new HashSet<Type>(supportedMessages.Select(m => m.Root));
        }

        private static bool IsConsumer(Type candidate)
        {
            var candidateGenericTypeDefinition = candidate.GetGenericTypeDefinition();

            return ConsumerInterfaces.Any(i => i.IsAssignableFrom(candidateGenericTypeDefinition));
        }
    }
}