using System;
using System.Collections.Generic;
using Common.Logging;

namespace Roger.Internal.Impl
{
    class AlwaysSuccessConsumerInvoker : IConsumerInvoker
    {
        private readonly ILog log = LogManager.GetCurrentClassLogger();

        public bool Invoke(IEnumerable<IConsumer> consumers, CurrentMessageInformation message)
        {
            foreach (var consumer in consumers)
            {
                log.TraceFormat("Invoking Consume method on consumer {0} for message {1}", consumer.GetType(), message.MessageType);

                try
                {
                    consumer.InvokePreservingStackTrace(message.Body);
                }
                catch (Exception e)
                {
                    log.Error("Exception while invoking consumer", e);
                }
            }

            return true;
        }
    }
}