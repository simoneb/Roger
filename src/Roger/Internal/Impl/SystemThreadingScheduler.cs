using System;
using System.Threading;

namespace Roger.Internal.Impl
{
    internal class SystemThreadingScheduler : IScheduler
    {
        public TimeSpan Interval { get; private set; }
        private readonly Timer timer;
        private int disposed;
        private static readonly TimeSpan Never = TimeSpan.FromMilliseconds(-1);

        public SystemThreadingScheduler(TimeSpan interval)
        {
            Interval = interval;
            timer = new Timer(OnCallback);
        }

        public void Start(TimeSpan? startIn = null)
        {
            if (Disposed)
                return;

            timer.Change(startIn ?? TimeSpan.Zero, Interval);
        }

        private void OnCallback(object state)
        {
            if(Disposed)
                return;

            Callback();
        }

        public event Action Callback = delegate {  };

        public void Stop()
        {
            if (Disposed)
                return;

            timer.Change(Never, Never);
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) == 1)
                return;

            Stop();
            timer.Dispose();
        }

        private bool Disposed
        {
            get { return disposed == 1; }
        }
    }
}