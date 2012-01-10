using System;
using System.Collections.Concurrent;
using System.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Roger.Internal.Impl
{
    internal class PublisherConfirmsModule : IPublishModule
    {
        private NullLog log;
        private readonly ConcurrentDictionary<ulong, IUnconfirmedCommandFactory> unconfirmedCommands = new ConcurrentDictionary<ulong, IUnconfirmedCommandFactory>();
        private readonly TimeSpan? consideredUnconfirmedAfter;
        private IPublishingProcess publisher;

        public PublisherConfirmsModule(TimeSpan? consideredUnconfirmedAfter = null)
        {
            this.consideredUnconfirmedAfter = consideredUnconfirmedAfter;
        }

        public void ConnectionEstablished(IModel publishModel)
        {
            publishModel.BasicAcks += PublishModelOnBasicAcks;
            publishModel.BasicNacks += PublishModelOnBasicNacks;
            publishModel.ConfirmSelect();
            log = new NullLog();
        }

        public void BeforePublish(IDeliveryCommand command, IModel publishModel)
        {
            unconfirmedCommands.TryAdd(publishModel.NextPublishSeqNo, new UnconfirmedCommandFactory(command, consideredUnconfirmedAfter));
        }

        public void Initialize(IPublishingProcess publishingProcess)
        {
            publisher = publishingProcess;
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

        internal void ProcessUnconfirmed()
        {
            var toProcess = unconfirmedCommands.Where(p => p.Value.CanExecute);

            foreach (var tp in toProcess)
            {
                IUnconfirmedCommandFactory publish;
                unconfirmedCommands.TryRemove(tp.Key, out publish);
                publisher.Process(publish);
            }
        }
    }
}