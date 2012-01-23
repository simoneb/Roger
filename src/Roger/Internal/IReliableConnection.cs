using System;
using RabbitMQ.Client;

namespace Roger.Internal
{
    internal interface IReliableConnection : IDisposable
    {
        TimeSpan ConnectionAttemptInterval { get; }
        void Connect();
        IModel CreateModel();
        event Action ConnectionAttemptFailed;
        event Action<ShutdownEventArgs> UnexpectedShutdown;
        event Action GracefulShutdown;
        event Action ConnectionEstabilished;
    }
}