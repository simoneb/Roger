using System;
using RabbitMQ.Client;
using Rabbus.Utilities;

namespace Rabbus.Internal.Impl
{
    internal class UnconfirmedCommandFactoryFactory : IUnconfirmedCommandFactory
    {
        private readonly IDeliveryCommand inner;
        private readonly TimeSpan republishUnconfirmedMessagesThreshold;
        private readonly DateTimeOffset created;

        public UnconfirmedCommandFactoryFactory(IDeliveryCommand inner, TimeSpan republishUnconfirmedMessagesThreshold)
        {
            created = SystemTime.Now;
            this.inner = inner;
            this.republishUnconfirmedMessagesThreshold = republishUnconfirmedMessagesThreshold;
        }

        public bool CanExecute
        {
            get { return SystemTime.Now - created >= republishUnconfirmedMessagesThreshold; }
        }

        public IDeliveryCommand Create(IModel model, IIdGenerator idGenerator, ITypeResolver typeResolver, IMessageSerializer serializer, ISequenceGenerator sequenceGenerator)
        {
            return inner;
        }
    }
}