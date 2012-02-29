using System;
using RabbitMQ.Client;
using Roger.Utilities;

namespace Roger.Internal.Impl
{
    internal class UnconfirmedDeliveryFactory : IUnconfirmedDeliveryFactory
    {
        private readonly IDelivery inner;
        private readonly TimeSpan? consideredUnconfirmedAfter;
        private readonly DateTimeOffset created;

        public UnconfirmedDeliveryFactory(IDelivery inner, TimeSpan? consideredUnconfirmedAfter)
        {
            created = SystemTime.Now;
            this.inner = inner;
            this.consideredUnconfirmedAfter = consideredUnconfirmedAfter;
        }

        public bool CanExecute
        {
            get { return !consideredUnconfirmedAfter.HasValue || SystemTime.Now >= created + consideredUnconfirmedAfter; }
        }

        public IDelivery Create(IModel model,
                                IIdGenerator idGenerator,
                                IMessageTypeResolver messageTypeResolver,
                                IMessageSerializer serializer,
                                ISequenceGenerator sequenceGenerator)
        {
            return inner;
        }
    }
}