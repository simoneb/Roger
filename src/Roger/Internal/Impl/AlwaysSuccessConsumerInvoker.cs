using System;
using System.Collections.Generic;

namespace Roger.Internal.Impl
{
    class AlwaysSuccessConsumerInvoker : IConsumerInvoker
    {
        private readonly IReflection reflection;
        private readonly NullLog log;

        public AlwaysSuccessConsumerInvoker(IReflection reflection)
        {
            this.reflection = reflection;
            log = new NullLog();
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
                    log.ErrorFormat("Exception while invoking consumer\r\n{0}", e);
                }
            }

            return true;
        }
    }
}