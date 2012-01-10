using System;
using System.Collections;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal abstract class AbstractDeliveryCommandFactory : IDeliveryCommandFactory
    {
        private readonly Type messageType;

        protected AbstractDeliveryCommandFactory(Type messageType)
        {
            this.messageType = messageType;
        }

        protected Func<RogerEndpoint, IBasicProperties> CreateProperties(IModel model,
                                                                         IIdGenerator idGenerator,
                                                                         ITypeResolver typeResolver,
                                                                         IMessageSerializer serializer,
                                                                         ISequenceGenerator sequenceGenerator,
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

            foreach (var additionalAction in additionalActions)
                additionalAction(properties);

            return endpoint =>
            {
                properties.ReplyTo = endpoint;

                return properties;
            };
        }

        public abstract IDeliveryCommand Create(IModel model,
                                                IIdGenerator idGenerator,
                                                ITypeResolver typeResolver,
                                                IMessageSerializer serializer,
                                                ISequenceGenerator sequenceGenerator);
    }
}