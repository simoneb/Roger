using System;
using System.Threading;

namespace Roger.Internal.Impl
{
    internal class DefaultWaiter : IWaiter
    {
        public bool Wait(WaitHandle waitHandle, TimeSpan timeout)
        {
            return waitHandle.WaitOne(timeout);
        }
    }
}