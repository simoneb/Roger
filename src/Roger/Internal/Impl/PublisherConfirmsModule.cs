using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Roger.Internal.Impl
{
    internal class PublisherConfirmsModule : IPublishModule
    {
        private readonly ILog log = LogManager.GetCurrentClassLogger();
        private readonly ConcurrentDictionary<ulong, IUnconfirmedDeliveryFactory> unconfirmedCommands = new ConcurrentDictionary<ulong, IUnconfirmedDeliveryFactory>();
        private readonly IScheduler scheduler;
        private readonly TimeSpan? consideredUnconfirmedAfter;
        private IPublishingProcess publisher;
        private int disposed;

        public PublisherConfirmsModule(IScheduler scheduler, TimeSpan? consideredUnconfirmedAfter = null)
        {
            this.scheduler = scheduler;
            this.consideredUnconfirmedAfter = consideredUnconfirmedAfter;
        }

        public void Initialize(IPublishingProcess publishingProcess)
        {
            publisher = publishingProcess;
            scheduler.Callback += ProcessUnconfirmed;
        }

        public void BeforePublishEnabled(IModel publishModel)
        {
            publishModel.BasicAcks += PublishModelOnBasicAcks;
            publishModel.BasicNacks += PublishModelOnBasicNacks;
            publishModel.ConfirmSelect();
            
            scheduler.Start();
        }

        public void BeforePublish(IDelivery command, IModel publishModel, IBasicProperties properties, Action<BasicReturn> basicReturnCallback)
        {
            unconfirmedCommands.TryAdd(publishModel.NextPublishSeqNo, new UnconfirmedDeliveryFactory(command, consideredUnconfirmedAfter));
        }

        public void AfterPublishDisabled(IModel publishModel)
        {
            scheduler.Stop();

            // make sure we don't receive unwanted events
            publishModel.BasicAcks -= PublishModelOnBasicAcks;
            publishModel.BasicNacks -= PublishModelOnBasicNacks;

            ForceProcessUnconfirmed();
        }

        private void PublishModelOnBasicAcks(IModel model, BasicAckEventArgs args)
        {
            IUnconfirmedDeliveryFactory _;

            if (args.Multiple)
            {
                log.DebugFormat("Broker confirmed all deliveries up to and including #{0}", args.DeliveryTag);

                var toRemove = unconfirmedCommands.Keys.Where(tag => tag <= args.DeliveryTag).ToArray();

                foreach (var tag in toRemove)
                    unconfirmedCommands.TryRemove(tag, out _);
            }
            else
            {
                log.DebugFormat("Broker confirmed delivery #{0}", args.DeliveryTag);
                unconfirmedCommands.TryRemove(args.DeliveryTag, out _);
            }

            log.DebugFormat("Left to be confirmed: {0}", unconfirmedCommands.Count);
        }

        private void PublishModelOnBasicNacks(IModel model, BasicNackEventArgs args)
        {
            if (args.Multiple)
                log.WarnFormat("Broker nacked all deliveries up to and including #{0}", args.DeliveryTag);
            else
                log.WarnFormat("Broker nacked delivery #{0}", args.DeliveryTag);
        }

        private void ProcessUnconfirmed()
        {
            ProcessCommands(unconfirmedCommands.Where(p => p.Value.CanExecute).Select(p => p.Key).ToArray());
        }

        private void ForceProcessUnconfirmed()
        {
            ProcessCommands(unconfirmedCommands.Keys);
        }

        private void ProcessCommands(ICollection<ulong> toProcess)
        {
            if (toProcess.Any())
            {
                log.InfoFormat("Processing {0} unconfirmed messages", toProcess.Count);

                foreach (var tp in toProcess)
                {
                    IUnconfirmedDeliveryFactory publish;

                    if(unconfirmedCommands.TryRemove(tp, out publish))
                        publisher.Process(publish);
                }
            }
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) == 1)
                return;

            scheduler.Callback -= ProcessUnconfirmed;
        }
    }
}