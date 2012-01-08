using System;
using System.Collections.Generic;

namespace Roger.Internal
{
    internal interface ISupportedMessageTypesResolver
    {
        ISet<Type> Resolve(Type consumerType);
    }
}