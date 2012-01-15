using System;

namespace Roger.Internal
{
    public interface IPublishingProcess : IDisposable
    {
        void Start();
        void Publish(object message, bool persistent);
        void Request(object message, Action<BasicReturn> basicReturnCallback, bool persistent);
        void Send(RogerEndpoint recipient, object message, Action<BasicReturn> basicReturnCallback, bool persistent);
        void PublishMandatory(object message, Action<BasicReturn> basicReturnCallback, bool persistent);
        void Reply(object message, CurrentMessageInformation request, Action<BasicReturn> basicReturnCallback, bool persistent);
        void Process(IDeliveryFactory factory);
    }
}