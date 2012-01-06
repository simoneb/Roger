using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Roger.Utilities
{
    internal class ConcurrentSelfExpiringCache<T> : ICache<T>
    {
        private class ExpiryDescriptor
        {
            private readonly TimeSpan expiry;
            private readonly DateTimeOffset added;

            public ExpiryDescriptor(TimeSpan expiry)
            {
                this.expiry = expiry;
                added = SystemTime.Now;
            }

            public bool Expired
            {
                get { return added < SystemTime.Now - expiry; }
            }
        }

        private readonly TimeSpan expiry;
        private readonly ConcurrentDictionary<T, ExpiryDescriptor> cache = new ConcurrentDictionary<T, ExpiryDescriptor>();

        public ConcurrentSelfExpiringCache(TimeSpan expiry)
        {
            this.expiry = expiry;
        }

        public bool TryAdd(T key)
        {
            var created = false;

            cache.AddOrUpdate(key, k => { created = true; return new ExpiryDescriptor(expiry); }, (k, _) => new ExpiryDescriptor(expiry));

            CleanupExpiredEntries();

            return created;
        }

        private void CleanupExpiredEntries()
        {
            var toRemove = cache.Where(c => c.Value.Expired).Select(p => p.Key).ToArray();

            foreach (var candidate in toRemove)
            {
                ExpiryDescriptor _;
                cache.TryRemove(candidate, out _);
            }
        }
    }
}