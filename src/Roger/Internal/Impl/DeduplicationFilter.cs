using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using Roger.Utilities;

namespace Roger.Internal.Impl
{
    internal class DeduplicationFilter : IMessageFilter
    {
        private readonly ICache<RogerGuid> cache;

        public DeduplicationFilter(ICache<RogerGuid> cache)
        {
            this.cache = cache;
        }

        public DeduplicationFilter(TimeSpan expiry)
        {
            cache = new ConcurrentSelfExpiringCache<RogerGuid>(expiry);
        }

        public IEnumerable<CurrentMessageInformation> Filter(IEnumerable<CurrentMessageInformation> input, IModel model)
        {
            foreach (var message in input)
            {
                if (cache.TryAdd(message.MessageId)) 
                    yield return message;
                else
                    model.BasicAck(message.DeliveryTag, false);
            }
        }
    }
}