using System;

namespace Roger.Internal
{
    internal interface ITimer : IDisposable
    {
        event Action Callback;
        void Start(TimeSpan? startIn = null);
        void Stop();
    }
}