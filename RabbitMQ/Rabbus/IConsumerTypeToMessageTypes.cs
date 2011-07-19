using System;
using System.Collections.Generic;

namespace Rabbus
{
    public interface IConsumerTypeToMessageTypes
    {
        IEnumerable<Type> Get(Type consumerType);
    }
}