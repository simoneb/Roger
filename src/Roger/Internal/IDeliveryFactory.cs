using RabbitMQ.Client;

namespace Roger.Internal
{
    /// <summary>
    /// Factories of deliveries
    /// </summary>
    public interface IDeliveryFactory
    {
        IDelivery Create(IModel model,
                         IIdGenerator idGenerator,
                         ITypeResolver typeResolver,
                         IMessageSerializer serializer,
                         ISequenceGenerator sequenceGenerator);
    }
}