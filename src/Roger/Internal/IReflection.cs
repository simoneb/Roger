using System;
using System.Collections.Generic;

namespace Roger.Internal
{
    internal interface IReflection
    {
        void InvokeConsume(IConsumer consumer, object message);
        IEnumerable<Type> Hierarchy(Type type);
    }
}