using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using Rabbus.Utilities;

namespace Rabbus.Internal.Impl
{
    internal class DeduplicationFilter : IMessageFilter
    {
        private readonly ICache<RabbusGuid> cache;

        public DeduplicationFilter(ICache<RabbusGuid> cache)
        {
            this.cache = cache;
        }

        public DeduplicationFilter(TimeSpan expiry)
        {
            cache = new ConcurrentSelfExpiringCache<RabbusGuid>(expiry);
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