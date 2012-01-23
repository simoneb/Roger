using System;
using System.Collections.Generic;

namespace Roger.Internal.Impl
{
    internal class EmptyConsumerContainer : IConsumerContainer
    {
        public IEnumerable<IConsumer> Resolve(Type messageRoot)
        {
            yield break;
        }

        public void Release(IEnumerable<IConsumer> consumers)
        {}

        public IEnumerable<Type> GetAllConsumerTypes()
        {
            yield break;
        }
    }
}