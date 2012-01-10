using System;

namespace Roger.Internal
{
    public interface ITimer : IDisposable
    {
        event Action Callback;
        void Start();
        void Stop();
    }
}