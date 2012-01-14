using System;
using System.Collections.Generic;
using Common.Logging;
using RabbitMQ.Client;
using Roger.Utilities;

namespace Roger.Internal.Impl
{
    internal class DeduplicationFilter : IMessageFilter
    {
        private readonly IRabbitBus bus;
        private readonly ICache<RogerGuid> cache;
        private readonly ILog log = LogManager.GetCurrentClassLogger();

        public DeduplicationFilter(IRabbitBus bus, TimeSpan expiry, int evictEveryNAdditions = 100) : this(bus, new ConcurrentSelfExpiringCache<RogerGuid>(expiry, evictEveryNAdditions))
        {
        }

        public DeduplicationFilter(IRabbitBus bus, ICache<RogerGuid> cache)
        {
            this.bus = bus;
            this.cache = cache;

            bus.Started += BusStarted;
            bus.Interrupted += BusInterrupted;
            bus.Stopped += BusStopped;
        }

        private void BusStopped()
        {
            bus.Started -= BusStarted;
            bus.Interrupted -= BusInterrupted;
            bus.Stopped -= BusStopped;
        }

        private void BusInterrupted()
        {
            cache.PauseEvictions();
        }

        private void BusStarted()
        {
            cache.ResumeEvictions();
        }

        public IEnumerable<CurrentMessageInformation> Filter(IEnumerable<CurrentMessageInformation> input, IModel model)
        {
            foreach (var message in input)
            {
                if (cache.TryAdd(message.MessageId)) 
                    yield return message;
                else
                {
                    log.InfoFormat("Filtered out message {0}", message.MessageId);
                    model.BasicAck(message.DeliveryTag, false);
                }
            }
        }
    }
}