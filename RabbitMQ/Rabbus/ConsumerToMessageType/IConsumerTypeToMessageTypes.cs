using System;
using System.Collections.Generic;

namespace Rabbus.ConsumerToMessageType
{
    public interface IConsumerTypeToMessageTypes
    {
        IEnumerable<Type> Get(Type consumerType);
    }
}