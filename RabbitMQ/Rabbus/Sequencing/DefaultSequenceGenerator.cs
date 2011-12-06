using System.Threading;

namespace Rabbus.Sequencing
{
    public class DefaultSequenceGenerator : ISequenceGenerator
    {
        private int currentSequence;

        public uint Next()
        {
            return (uint) Interlocked.Increment(ref currentSequence);
        }
    }
}