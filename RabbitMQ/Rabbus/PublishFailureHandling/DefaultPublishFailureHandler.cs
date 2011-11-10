using System;
using System.Collections.Concurrent;
using System.Linq;
using Rabbus.Errors;
using Rabbus.GuidGeneration;
using Rabbus.Logging;

namespace Rabbus.PublishFailureHandling
{
    internal class DefaultPublishFailureHandler : IPublishFailureHandler
    {
        private readonly IRabbusLog log;

        private readonly ConcurrentDictionary<RabbusGuid, Func<PublishFailureReason, bool>> callbacks =
            new ConcurrentDictionary<RabbusGuid, Func<PublishFailureReason, bool>>();

        public DefaultPublishFailureHandler(IRabbusLog log)
        {
            this.log = log;
        }

        public void Subscribe(RabbusGuid messageId, Action<PublishFailureReason> callback)
        {
            callbacks.TryAdd(messageId, HandleReturn(new WeakReference(callback), messageId));
        }

        public void Handle(PublishFailureReason publishFailureReason)
        {
            var toRemove = callbacks.Where(pair => pair.Value(publishFailureReason)).Select(p => p.Key).ToArray();

            log.DebugFormat("Removing {0} return callbacks from internal storage", toRemove.Length);

            foreach (var callback in toRemove)
            {
                Func<PublishFailureReason, bool> _;
                callbacks.TryRemove(callback, out _);
            }
        }

        private Func<PublishFailureReason, bool> HandleReturn(WeakReference callback, string messageId)
        {
            var myMessageId = new RabbusGuid(messageId);

            return reason =>
            {
                if (!callback.IsAlive)
                    return true;

                if (reason.MessageId == myMessageId)
                {
                    log.DebugFormat("Invoking basic return callback for message id {0}", reason.MessageId);
                    ((Action<PublishFailureReason>)callback.Target)(reason);
                    return true;
                }

                return false;
            };
        }
    }
}