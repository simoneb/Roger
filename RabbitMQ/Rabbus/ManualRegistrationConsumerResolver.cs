using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Rabbus.ConsumerToMessageType;

namespace Rabbus
{
    public class ManualRegistrationConsumerResolver : IConsumerResolver
    {
        private readonly IConsumerTypeToMessageTypes consumerTypeToMessageTypes;
        readonly ConcurrentDictionary<Type, List<IConsumer>> messageTypeToConsumers = new ConcurrentDictionary<Type, List<IConsumer>>();

        public ManualRegistrationConsumerResolver(IConsumerTypeToMessageTypes consumerTypeToMessageTypes)
        {
            this.consumerTypeToMessageTypes = consumerTypeToMessageTypes;
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
        }

        public IEnumerable<Type> GetAllConsumersTypes()
        {
            return messageTypeToConsumers.SelectMany(c => c.Value.Select(v => v.GetType()));
        }

        public void Register(IConsumer consumer)
        {
            foreach (var supportedMessageType in consumerTypeToMessageTypes.Get(consumer.GetType()))
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