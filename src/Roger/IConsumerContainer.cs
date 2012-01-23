using System;
using System.Collections.Generic;

namespace Roger
{
    public interface IConsumerContainer
    {
        IEnumerable<IConsumer> Resolve(Type messageRoot);
        void Release(IEnumerable<IConsumer> consumers);
        IEnumerable<Type> GetAllConsumerTypes();
    }
}