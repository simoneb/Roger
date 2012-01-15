using System;
using System.Collections;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal abstract class AbstractDeliveryFactory : IDeliveryFactory
    {
        private readonly Type messageType;

        protected AbstractDeliveryFactory(Type messageType)
        {
            this.messageType = messageType;
        }

        protected Func<RogerEndpoint, IBasicProperties> CreatePropertiesFactory(IModel model,
                                                                                IIdGenerator idGenerator,
                                                                                ITypeResolver typeResolver,
                                                                                IMessageSerializer serializer,
                                                                                ISequenceGenerator sequenceGenerator,
                                                                                bool persistent,
                                                                                params Action<IBasicProperties>[] additionalActions)
        {
            var properties = model.CreateBasicProperties();

            properties.MessageId = idGenerator.Next();
            properties.Type = typeResolver.Unresolve(messageType);
            properties.ContentType = serializer.ContentType;

            properties.Headers = new Hashtable
            {
                {Headers.Sequence, BitConverter.GetBytes(sequenceGenerator.Next())}
            };

            if (persistent)
                properties.DeliveryMode = 2;

            foreach (var additionalAction in additionalActions)
                additionalAction(properties);

            return endpoint =>
            {
                properties.ReplyTo = endpoint;
                return properties;
            };
        }

        public abstract IDelivery Create(IModel model,
                                         IIdGenerator idGenerator,
                                         ITypeResolver typeResolver,
                                         IMessageSerializer serializer,
                                         ISequenceGenerator sequenceGenerator);
    }
}