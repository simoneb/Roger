using System;
using System.Threading;
using Common.Logging;

namespace Roger.Internal.Impl
{
    internal class SystemThreadingTimer : ITimer
    {
        private readonly Timer timer;
        private int disposed;
        private static readonly TimeSpan Never = TimeSpan.FromMilliseconds(-1);
        private readonly ILog log = LogManager.GetCurrentClassLogger();

        public event Action Callback = delegate {  };

        public SystemThreadingTimer()
        {
            timer = new Timer(OnCallback);
        }

        private bool Disposed
        {
            get { return disposed == 1; }
        }

        public void Start(TimeSpan? startIn = null)
        {
            if (Disposed)
                return;

            log.DebugFormat("Will callback in {0}", startIn);
            timer.Change(startIn ?? TimeSpan.Zero, Never);
        }

        private void OnCallback(object state)
        {
            if(Disposed)
                return;

            log.Debug("Calling back");
            Callback();
        }

        public void Stop()
        {
            if (Disposed)
                return;

            log.Debug("Stopped");
            timer.Change(Never, Never);
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) == 1)
                return;

            Stop();
            timer.Dispose();
        }
    }
}