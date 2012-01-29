using System;
using RabbitMQ.Client;
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