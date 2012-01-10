using System;
using System.Threading;

namespace Roger.Internal.Impl
{
    internal class SystemThreadingTimer : ITimer
    {
        private readonly long interval;
        private readonly Timer timer;
        private int disposed;

        public SystemThreadingTimer(TimeSpan interval)
        {
            this.interval = (long)interval.TotalMilliseconds;
            timer = new Timer(OnCallback);
        }

        public void Start()
        {
            if (Disposed())
                return;

            timer.Change(interval, Timeout.Infinite);
        }

        private void OnCallback(object state)
        {
            if(Disposed())
                return;

            Callback();
        }

        public event Action Callback = delegate {  };

        public void Stop()
        {
            if (Disposed())
                return;

            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Dispose()
        {
            if (Disposed())
                return;

            Stop();
            timer.Dispose();
        }

        private bool Disposed()
        {
            return Interlocked.CompareExchange(ref disposed, 1, 0) == 1;
        }
    }
}