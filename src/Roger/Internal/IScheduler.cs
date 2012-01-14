using System;

namespace Roger.Internal
{
    public interface IScheduler : IDisposable
    {
        void Start(TimeSpan? startIn = null);
        event Action Callback;
        void Stop();
        TimeSpan Interval { get; }
    }
}