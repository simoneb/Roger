using System;
using RabbitMQ.Client;

namespace Roger.Internal
{
    public interface IReliableConnection : IDisposable
    {
        TimeSpan ConnectionAttemptInterval { get; }
        void Connect(Action onFirstConnection = null);
        IModel CreateModel();
        event Action ConnectionAttemptFailed;
        event Action<ShutdownEventArgs> UnexpectedShutdown;
        event Action GracefulShutdown;
        event Action ConnectionEstabilished;
    }
}