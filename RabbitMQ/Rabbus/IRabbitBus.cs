using System;

namespace Rabbus
{
    public interface IRabbitBus
    {
        IDisposable Subscribe(IConsumer consumer);
        void Publish(object message);
        void PublishMandatory(object message, Action<PublishFailureReason> publishFailure);
    }
}