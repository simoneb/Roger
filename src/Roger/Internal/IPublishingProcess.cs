using System;

namespace Roger.Internal
{
    internal interface IPublishingProcess : IDisposable
    {
        void Start();
        void Publish(object message, bool persistent, bool sequence);
        void Request(object message, Action<BasicReturn> basicReturnCallback, bool persistent, bool sequence);
        void Send(RogerEndpoint recipient, object message, Action<BasicReturn> basicReturnCallback, bool persistent, bool sequence);
        void PublishMandatory(object message, Action<BasicReturn> basicReturnCallback, bool persistent, bool sequence);
        void Reply(object message, CurrentMessageInformation currentMessage, Action<BasicReturn> basicReturnCallback, bool persistent, bool sequence);
        void Process(IDeliveryFactory factory);
    }
}