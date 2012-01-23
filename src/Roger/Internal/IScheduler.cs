using System;

namespace Roger.Internal
{
    internal interface IScheduler : IDisposable
    {
        void Start(TimeSpan? startIn = null);
        event Action Callback;
        void Stop();
        TimeSpan Interval { get; }
    }
}