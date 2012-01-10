using System;
using RabbitMQ.Client;
using Roger.Utilities;

namespace Roger.Internal.Impl
{
    internal class UnconfirmedCommandFactory : IUnconfirmedCommandFactory
    {
        private readonly IDeliveryCommand inner;
        private readonly TimeSpan? consideredUnconfirmedAfter;
        private readonly DateTimeOffset created;

        public UnconfirmedCommandFactory(IDeliveryCommand inner, TimeSpan? consideredUnconfirmedAfter)
        {
            created = SystemTime.Now;
            this.inner = inner;
            this.consideredUnconfirmedAfter = consideredUnconfirmedAfter;
        }

        public bool CanExecute
        {
            get { return !consideredUnconfirmedAfter.HasValue || SystemTime.Now >= created + consideredUnconfirmedAfter; }
        }

        public IDeliveryCommand Create(IModel model, IIdGenerator idGenerator, ITypeResolver typeResolver, IMessageSerializer serializer, ISequenceGenerator sequenceGenerator)
        {
            return inner;
        }
    }
}