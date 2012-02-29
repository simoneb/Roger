using RabbitMQ.Client;

namespace Roger.Internal
{
    internal interface IDeliveryFactory
    {
        IDelivery Create(IModel model,
                         IIdGenerator idGenerator,
                         IMessageTypeResolver messageTypeResolver,
                         IMessageSerializer serializer,
                         ISequenceGenerator sequenceGenerator);
    }
}