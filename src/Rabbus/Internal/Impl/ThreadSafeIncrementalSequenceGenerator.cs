using System.Threading;

namespace Rabbus.Internal.Impl
{
    internal class ThreadSafeIncrementalSequenceGenerator : ISequenceGenerator
    {
        private int currentSequence;

        public uint Next()
        {
            return (uint) Interlocked.Increment(ref currentSequence);
        }
    }
}