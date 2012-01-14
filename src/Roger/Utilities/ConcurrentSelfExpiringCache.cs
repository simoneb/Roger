using System;
using System.Collections.Concurrent;
using System.Linq;
using Common.Logging;

namespace Roger.Utilities
{
    internal class ConcurrentSelfExpiringCache<T> : ICache<T>
    {
        private class ExpiryDescriptor
        {
            private readonly TimeSpan expiry;
            private DateTimeOffset added;

            public ExpiryDescriptor(TimeSpan expiry)
            {
                this.expiry = expiry;
                added = SystemTime.Now;
            }

            public bool Expired
            {
                get { return added < SystemTime.Now - expiry; }
            }

            public void ResetExpiry()
            {
                added = SystemTime.Now;
            }
        }

        private readonly TimeSpan expiry;
        private readonly int evictEveryNAdditions;
        private readonly ConcurrentDictionary<T, ExpiryDescriptor> cache = new ConcurrentDictionary<T, ExpiryDescriptor>();
        private readonly ILog log = LogManager.GetCurrentClassLogger();
        private bool pause;

        public ConcurrentSelfExpiringCache(TimeSpan expiry, int evictEveryNAdditions)
        {
            this.expiry = expiry;
            this.evictEveryNAdditions = evictEveryNAdditions;
        }

        public bool TryAdd(T key)
        {
            TryCleanupExpiredEntries();

            return AddToCache(key);
        }

        private bool AddToCache(T key)
        {
            var created = false;

            cache.AddOrUpdate(key, 
                              k => { created = true; return new ExpiryDescriptor(expiry); }, 
                              (k, _) => new ExpiryDescriptor(expiry));

            return created;
        }

        private void TryCleanupExpiredEntries()
        {
            if (pause)
                return;

            if (cache.Count % evictEveryNAdditions != 0)
                return;

            CleanupExpiredEntries();
        }

        public void PauseEvictions()
        {
            pause = true;
        }

        public void ResumeEvictions()
        {
            cache.ForEach(c => c.Value.ResetExpiry());
            pause = false;
        }

        private void CleanupExpiredEntries()
        {
            var toRemove = cache.Where(c => c.Value.Expired).Select(p => p.Key).ToArray();

            log.InfoFormat("Removing {0} items out of {1}", toRemove.Length, cache.Count);

            foreach (var candidate in toRemove)
            {
                ExpiryDescriptor _;
                cache.TryRemove(candidate, out _);
            }
        }
    }
}