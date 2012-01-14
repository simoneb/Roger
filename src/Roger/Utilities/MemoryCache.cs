using System;
using System.Runtime.Caching;
using Roger.Internal;

namespace Roger.Utilities
{
    class MemoryCache<T> : ICache<T>, IDisposable
    {
        private readonly TimeSpan expiry;
        private readonly MemoryCache inner;
        private readonly IScheduler resurrector;
        private bool pausingEvictions;

        public MemoryCache(TimeSpan expiry, IScheduler resurrector)
        {
            this.expiry = expiry;
            this.resurrector = resurrector;

            resurrector.Callback += ResurrectItems;
            inner = new MemoryCache(Guid.NewGuid().ToString());
        }

        private void ResurrectItems()
        {
            inner.ForEach(pair => inner.Get(pair.Key));
        }

        public bool TryAdd(T key)
        {
            return inner.Add(key.ToString(), key, new CacheItemPolicy {SlidingExpiration = expiry});
        }

        public void PauseEvictions()
        {
            if (pausingEvictions)
                return;

            pausingEvictions = true;

            resurrector.Start();
        }

        public void ResumeEvictions()
        {
            if (!pausingEvictions)
                return;

            pausingEvictions = false;

            resurrector.Stop();
        }

        public void Dispose()
        {
            resurrector.Stop();
            inner.Dispose();
        }
    }
}