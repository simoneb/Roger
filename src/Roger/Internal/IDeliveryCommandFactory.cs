using RabbitMQ.Client;

namespace Roger.Internal
{
    public interface IDeliveryCommandFactory
    {
        IDeliveryCommand Create(IModel model, IIdGenerator idGenerator, ITypeResolver typeResolver, IMessageSerializer serializer, ISequenceGenerator sequenceGenerator);
    }
}