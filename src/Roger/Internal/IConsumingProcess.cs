using System;

namespace Rabbus.Internal
{
    internal interface IConsumingProcess : IDisposable
    {
        IDisposable AddInstanceSubscription(IConsumer consumer);
        void Consume(object message);
        CurrentMessageInformation CurrentMessage { get; }
        RabbusEndpoint Endpoint { get; }
    }
}