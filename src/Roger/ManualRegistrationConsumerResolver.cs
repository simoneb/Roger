using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Rabbus
{
    public class ManualRegistrationConsumerResolver : IConsumerResolver
    {
        private readonly ISupportedMessageTypesResolver supportedMessageTypesResolver;
        readonly ConcurrentDictionary<Type, List<IConsumer>> messageTypeToConsumers = new ConcurrentDictionary<Type, List<IConsumer>>();

        public ManualRegistrationConsumerResolver(ISupportedMessageTypesResolver supportedMessageTypesResolver)
        {
            this.supportedMessageTypesResolver = supportedMessageTypesResolver;
        }

        public IEnumerable<IConsumer> Resolve(Type messageType)
        {
            List<IConsumer> supportedConsumers;

            return messageTypeToConsumers.TryGetValue(messageType, out supportedConsumers)
                       ? supportedConsumers
                       : Enumerable.Empty<IConsumer>();
        }

        public void Release(IEnumerable<IConsumer> consumers)
        {
            // nothing to do here
        }

        public ISet<Type> GetAllSupportedMessageTypes()
        {
            return new HashSet<Type>(messageTypeToConsumers.Keys);
        }

        public void Register(IConsumer consumer)
        {
            foreach (var supportedMessageType in supportedMessageTypesResolver.Resolve(consumer.GetType()))
            {
                messageTypeToConsumers.AddOrUpdate(supportedMessageType,
                                                   new List<IConsumer> {consumer},
                                                   (type, list) =>
                                                   {
                                                       list.Add(consumer);
                                                       return list;
                                                   });
            }
        }
    }
}