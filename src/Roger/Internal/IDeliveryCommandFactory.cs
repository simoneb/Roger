using RabbitMQ.Client;

namespace Rabbus.Internal
{
    internal interface IDeliveryCommandFactory
    {
        IDeliveryCommand Create(IModel model, IIdGenerator idGenerator, ITypeResolver typeResolver, IMessageSerializer serializer, ISequenceGenerator sequenceGenerator);
    }
}