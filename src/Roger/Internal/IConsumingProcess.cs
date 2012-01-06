using System;

namespace Roger.Internal
{
    internal interface IConsumingProcess : IDisposable
    {
        IDisposable AddInstanceSubscription(IConsumer consumer);
        void Consume(object message);
        CurrentMessageInformation CurrentMessage { get; }
        RogerEndpoint Endpoint { get; }
    }
}