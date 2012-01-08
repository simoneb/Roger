using System;
using System.Collections.Generic;

namespace Roger.Internal
{
    public interface IReflection
    {
        void InvokeConsume(IConsumer consumer, object message);
        IEnumerable<Type> Hierarchy(Type type);
    }
}