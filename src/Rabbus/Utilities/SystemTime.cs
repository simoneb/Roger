using System;

namespace Rabbus.Utilities
{
    internal static class SystemTime
    {
        private static DateTimeOffset? _now;

        public static DateTimeOffset Now
        {
            get { return _now ?? DateTimeOffset.Now; }
            private set { _now = value; }
        }

        /// <summary>
        /// Freezes the time to the current moment until Dispose is called
        /// </summary>
        /// <returns></returns>
        public static IDisposable Freeze()
        {
            Now = DateTime.Now;
            return new DisposableAction(Reset);
        }

        /// <summary>
        /// Freezes the time to the value specified in <paramref name="freezeAt"/> until Dispose is called
        /// </summary>
        /// <returns></returns>
        public static IDisposable Freeze(DateTimeOffset freezeAt)
        {
            Now = freezeAt;
            return new DisposableAction(Reset);
        }

        /// <summary>
        /// Moves the time back of <paramref name="howMuchBackInTime"/> with respect to the current time
        /// </summary>
        /// <param name="howMuchBackInTime"></param>
        /// <returns></returns>
        public static IDisposable GoBack(TimeSpan howMuchBackInTime)
        {
            Now -= howMuchBackInTime;
            return new DisposableAction(Reset);
        }

        public static IDisposable GoForward(TimeSpan howMuchForwardInTheFuture)
        {
            Now += howMuchForwardInTheFuture;
            return new DisposableAction(Reset);
        }

        public static void Reset()
        {
            _now = null;
        }
    }
}