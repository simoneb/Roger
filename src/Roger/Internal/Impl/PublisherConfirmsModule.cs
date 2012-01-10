using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Roger.Internal.Impl
{
    internal class PublisherConfirmsModule : IPublishModule
    {
        private readonly IRogerLog log;
        private readonly ConcurrentDictionary<ulong, IUnconfirmedCommandFactory> unconfirmedCommands = new ConcurrentDictionary<ulong, IUnconfirmedCommandFactory>();
        private readonly ITimer timer;
        private readonly TimeSpan? consideredUnconfirmedAfter;
        private IPublishingProcess publisher;
        private int disposed;

        public PublisherConfirmsModule(ITimer timer, TimeSpan? consideredUnconfirmedAfter = null)
        {
            this.timer = timer;
            this.consideredUnconfirmedAfter = consideredUnconfirmedAfter;
            log = new NullLog();
        }

        public void Initialize(IPublishingProcess publishingProcess)
        {
            publisher = publishingProcess;
            timer.Callback += ProcessUnconfirmed;
        }

        public void ConnectionEstablished(IModel publishModel)
        {
            publishModel.BasicAcks += PublishModelOnBasicAcks;
            publishModel.BasicNacks += PublishModelOnBasicNacks;
            publishModel.ConfirmSelect();

            ForceProcessUnconfirmed();

            timer.Start();
        }

        public void BeforePublish(IDeliveryCommand command, IModel publishModel, IBasicProperties properties, Action<BasicReturn> basicReturnCallback)
        {
            unconfirmedCommands.TryAdd(publishModel.NextPublishSeqNo, new UnconfirmedCommandFactory(command, consideredUnconfirmedAfter));
        }

        public void ConnectionUnexpectedShutdown()
        {
            timer.Stop();
        }

        private void PublishModelOnBasicAcks(IModel model, BasicAckEventArgs args)
        {
            IUnconfirmedCommandFactory _;

            if (args.Multiple)
            {
                log.DebugFormat("Broker confirmed all deliveries up to and including {0}", args.DeliveryTag);

                var toRemove = unconfirmedCommands.Keys.Where(tag => tag <= args.DeliveryTag).ToArray();

                foreach (var tag in toRemove)
                    unconfirmedCommands.TryRemove(tag, out _);
            }
            else
            {
                log.DebugFormat("Broker confirmed delivery {0}", args.DeliveryTag);
                unconfirmedCommands.TryRemove(args.DeliveryTag, out _);
            }

            log.DebugFormat("Deliveries yet to be confirmed: {0}", unconfirmedCommands.Count);
        }

        private void PublishModelOnBasicNacks(IModel model, BasicNackEventArgs args)
        {
            if (args.Multiple)
                log.WarnFormat("Broker nacked all deliveries up to and including {0}", args.DeliveryTag);
            else
                log.WarnFormat("Broker nacked delivery {0}", args.DeliveryTag);
        }

        private void ProcessUnconfirmed()
        {
            ProcessCommands(unconfirmedCommands.Where(p => p.Value.CanExecute));
        }

        private void ForceProcessUnconfirmed()
        {
            ProcessCommands(unconfirmedCommands);
        }

        private void ProcessCommands(IEnumerable<KeyValuePair<ulong, IUnconfirmedCommandFactory>> toProcess)
        {
            log.Info("Processing unconfirmed messages");

            foreach (var tp in toProcess)
            {
                IUnconfirmedCommandFactory publish;
                unconfirmedCommands.TryRemove(tp.Key, out publish);
                publisher.Process(publish);
            }
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) == 1)
                return;

            timer.Callback -= ProcessUnconfirmed;
        }
    }
}