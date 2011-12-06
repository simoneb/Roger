using System;

namespace Rabbus.Consuming
{
    internal interface IConsumingProcess : IDisposable
    {
        IDisposable AddInstanceSubscription(IConsumer consumer);
        void Consume(object message);
        CurrentMessageInformation CurrentMessage { get; }
        RabbusEndpoint Endpoint { get; }
    }
}