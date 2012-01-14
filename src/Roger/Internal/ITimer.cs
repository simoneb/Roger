using System;

namespace Roger.Internal
{
    public interface ITimer : IDisposable
    {
        event Action Callback;
        void Start(TimeSpan? startIn = null);
        void Stop();
    }
}