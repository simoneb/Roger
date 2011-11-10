using System;
using Rabbus.Errors;
using Rabbus.GuidGeneration;

namespace Rabbus.PublishFailureHandling
{
    internal interface IPublishFailureHandler
    {
        void Handle(PublishFailureReason publishFailureReason);
        void Subscribe(RabbusGuid messageId, Action<PublishFailureReason> callback);
    }
}