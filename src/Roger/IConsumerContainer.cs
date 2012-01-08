using System;
using System.Collections.Generic;

namespace Roger
{
    public interface IConsumerContainer
    {
        IEnumerable<IConsumer> Resolve(Type consumerType);
        void Release(IEnumerable<IConsumer> consumers);
        IEnumerable<Type> GetAllConsumerTypes();
    }
}