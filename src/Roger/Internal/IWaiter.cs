using System;
using System.Threading;

namespace Roger.Internal
{
    /// <summary>
    /// Waits until a timeout occurs on a <see cref="WaitHandle"/> instance
    /// </summary>
    public interface IWaiter
    {
        bool Wait(WaitHandle waitHandle, TimeSpan timeout);
    }
}