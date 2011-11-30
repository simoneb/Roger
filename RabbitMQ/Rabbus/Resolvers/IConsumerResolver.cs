using System;
using System.Collections.Generic;

namespace Rabbus.Resolvers
{
    public interface IConsumerResolver
    {
        IEnumerable<IConsumer> Resolve(Type messageType);
        void Release(IEnumerable<IConsumer> consumers);
        HashSet<Type> GetAllSupportedMessageTypes();
    }
}