using System;
using Roger.Internal.Impl;

namespace Roger.Internal
{
    internal interface IReliableConnection : IDisposable
    {
        TimeSpan ConnectionAttemptInterval { get; }
        void Connect();
        IModelWithConnection CreateModel();
    }
}