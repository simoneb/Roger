using System;
using System.Collections.Generic;

namespace Rabbus
{
    public interface IConsumerToMessageTypes
    {
        IEnumerable<Type> Get(IConsumer consumer);
    }
}