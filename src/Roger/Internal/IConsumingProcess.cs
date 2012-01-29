using System;
using RabbitMQ.Client;
using Roger.Internal.Impl;

namespace Roger.Internal
{
    internal interface IConsumingProcess : IDisposable
    {
        IDisposable AddInstanceSubscription(IConsumer consumer);
        void Consume(object message);
        CurrentMessageInformation CurrentMessage { get; }
        RogerEndpoint Endpoint { get; }
        IModelWithConnection Model { get; }
    }
}