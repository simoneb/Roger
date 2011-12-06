using System;

namespace Rabbus.Consuming
{
    internal interface IConsumingProcess
    {
        IDisposable AddInstanceSubscription(IConsumer consumer);
        void Consume(object message);
        CurrentMessageInformation CurrentMessage { get; }
        RabbusEndpoint Endpoint { get; }
    }
}