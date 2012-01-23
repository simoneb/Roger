using RabbitMQ.Client;

namespace Roger.Internal
{
    internal interface IDeliveryFactory
    {
        IDelivery Create(IModel model,
                         IIdGenerator idGenerator,
                         ITypeResolver typeResolver,
                         IMessageSerializer serializer,
                         ISequenceGenerator sequenceGenerator);
    }
}