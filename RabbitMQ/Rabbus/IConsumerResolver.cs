using System;
using System.Collections.Generic;

namespace Rabbus
{
    public interface IConsumerResolver
    {
        IEnumerable<IConsumer> Resolve(Type messageType);
        void Release(IEnumerable<IConsumer> consumers);
        IEnumerable<Type> GetAllConsumersTypes();
    }
}