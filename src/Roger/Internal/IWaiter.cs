using System;
using System.Threading;

namespace Roger.Internal
{
    internal interface IWaiter
    {
        bool Wait(WaitHandle waitHandle, TimeSpan timeout);
    }
}