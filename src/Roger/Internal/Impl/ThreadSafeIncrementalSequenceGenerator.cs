using System.Threading;

namespace Roger.Internal.Impl
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