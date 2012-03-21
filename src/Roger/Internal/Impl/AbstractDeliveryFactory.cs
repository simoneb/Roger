using System;
using System.Collections;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal abstract class AbstractDeliveryFactory : IDeliveryFactory
    {
        private readonly Type messageType;
        private readonly bool persistent;
        private readonly bool sequence;

        protected AbstractDeliveryFactory(Type messageType, bool persistent, bool sequence)
        {
            this.messageType = messageType;
            this.persistent = persistent;
            this.sequence = sequence;
        }

        public IDelivery Create(IModel model, IIdGenerator idGenerator, IMessageTypeResolver messageTypeResolver, IMessageSerializer serializer, ISequenceGenerator sequenceGenerator)
        {
            var createProperties = CreatePropertiesFactory(model, idGenerator, messageTypeResolver, serializer, sequenceGenerator);

            return CreateCore(createProperties);
        }

        private Func<RogerEndpoint, IBasicProperties> CreatePropertiesFactory(IModel model,
                                                                              IIdGenerator idGenerator,
                                                                              IMessageTypeResolver messageTypeResolver,
                                                                              IMessageSerializer serializer,
                                                                              ISequenceGenerator sequenceGenerator)
        {
            var properties = model.CreateBasicProperties();

            properties.MessageId = idGenerator.Next();
            properties.Type = messageTypeResolver.Unresolve(messageType);
            properties.ContentType = serializer.ContentType;

            properties.Headers = new Hashtable();

            if(sequence)
                properties.Headers[Headers.Sequence] = BitConverter.GetBytes(sequenceGenerator.Next(messageType));

            if (persistent)
                properties.DeliveryMode = 2;

            FillAdditionalProperties(properties);

            return endpoint =>
            {
                properties.ReplyTo = endpoint;
                return properties;
            };
        }

        protected virtual void FillAdditionalProperties(IBasicProperties properties)
        {
        }

        protected abstract IDelivery CreateCore(Func<RogerEndpoint, IBasicProperties> createProperties);
    }
}