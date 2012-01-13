using System;
using System.Collections.Generic;
using Common.Logging;

namespace Roger.Internal.Impl
{
    class AlwaysSuccessConsumerInvoker : IConsumerInvoker
    {
        private readonly IReflection reflection;
        private readonly ILog log = LogManager.GetCurrentClassLogger();

        public AlwaysSuccessConsumerInvoker(IReflection reflection)
        {
            this.reflection = reflection;
        }

        public bool Invoke(IEnumerable<IConsumer> consumers, CurrentMessageInformation message)
        {
            foreach (var consumer in consumers)
            {
                log.DebugFormat("Invoking Consume method on consumer {0} for message {1}", consumer.GetType(), message.MessageType);

                try
                {
                    reflection.InvokeConsume(consumer, message.Body);
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