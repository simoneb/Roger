using System;
using System.Collections.Generic;

namespace Rabbus.Resolvers
{
    public interface ISupportedMessageTypesResolver
    {
        IEnumerable<Type> Get(Type consumerType);
    }
}