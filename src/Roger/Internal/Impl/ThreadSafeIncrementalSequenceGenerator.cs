using System;
using System.Collections.Concurrent;

namespace Roger.Internal.Impl
{
    internal class ThreadSafeIncrementalSequenceGenerator : ISequenceGenerator
    {
        readonly ConcurrentDictionary<Type, uint> sequences = new ConcurrentDictionary<Type, uint>();

        public uint Next(Type messageType)
        {
            return sequences.AddOrUpdate(messageType.HierarchyRoot(), type => 1u, (type, s) => s+1);
        }
    }
}