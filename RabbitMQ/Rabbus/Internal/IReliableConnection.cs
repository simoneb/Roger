using System;
using RabbitMQ.Client;

namespace Rabbus.Internal
{
    public interface IReliableConnection : IDisposable
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