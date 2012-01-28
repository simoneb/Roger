using System;
using RabbitMQ.Client;

namespace Roger.Internal
{
    internal interface IReliableConnection : IDisposable
    {
        TimeSpan ConnectionAttemptInterval { get; }
        void Connect();
        IModel CreateModel();
    }
}