using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Roger.Internal.Impl
{
    internal class DefaultBasicReturnHandler : IBasicReturnHandler
    {
        private readonly IRogerLog log;

        private readonly ConcurrentDictionary<RogerGuid, Func<BasicReturn, bool>> callbacks =
            new ConcurrentDictionary<RogerGuid, Func<BasicReturn, bool>>();

        public DefaultBasicReturnHandler(IRogerLog log)
        {
            this.log = log;
        }

        public void Subscribe(RogerGuid messageId, Action<BasicReturn> callback)
        {
            callbacks.TryAdd(messageId, WrapCallback(new WeakReference(callback), messageId));
        }

        public void Handle(BasicReturn basicReturn)
        {
            var toRemove = callbacks.Where(pair => pair.Value(basicReturn)).Select(p => p.Key).ToArray();

            log.DebugFormat("Removing {0} return callbacks from internal storage", toRemove.Length);

            foreach (var id in toRemove)
            {
                Func<BasicReturn, bool> _;
                callbacks.TryRemove(id, out _);
            }
        }

        private Func<BasicReturn, bool> WrapCallback(WeakReference callback, RogerGuid messageId)
        {
            return @return =>
            {
                if (!callback.IsAlive)
                    return true;

                if (@return.MessageId == messageId)
                {
                    log.DebugFormat("Invoking basic return callback for message id {0}", @return.MessageId);
                    ((Action<BasicReturn>)callback.Target)(@return);
                    return true;
                }

                return false;
            };
        }
    }
}