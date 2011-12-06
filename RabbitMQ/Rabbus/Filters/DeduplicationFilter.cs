using System;
using System.Collections.Generic;
using System.Linq;
using Rabbus.GuidGeneration;
using Rabbus.Utilities;

namespace Rabbus.Filters
{
    internal class DeduplicationFilter : IMessageFilter
    {
        private readonly ConcurrentSelfExpiringCache<RabbusGuid> cache;

        public DeduplicationFilter(TimeSpan expiry)
        {
            cache = new ConcurrentSelfExpiringCache<RabbusGuid>(expiry);
        }

        public IEnumerable<CurrentMessageInformation> Filter(IEnumerable<CurrentMessageInformation> input)
        {
            return input.Where(i => cache.TryAdd(i.MessageId));
        }
    }
}