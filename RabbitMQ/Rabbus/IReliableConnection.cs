using System;
using RabbitMQ.Client;

namespace Rabbus
{
    internal interface IReliableConnection : IDisposable
    {
        TimeSpan ConnectionAttemptInterval { get; }
        void Connect();
        IModel CreateModel();
    }
}