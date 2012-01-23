using System;

namespace Roger.Utilities
{
    internal static class SystemTime
    {
        private static DateTimeOffset? now;

        public static DateTimeOffset Now
        {
            get { return now ?? DateTimeOffset.Now; }
            private set { now = value; }
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
            now = null;
        }
    }
}