using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Rabbus;

namespace Tests.Bus
{
    public class FakeConsumerResolver : IConsumerResolver
    {
        private readonly IConsumerToMessageTypes m_consumerToMessageTypes;
        readonly ConcurrentDictionary<Type, List<IConsumer>> consumers = new ConcurrentDictionary<Type, List<IConsumer>>();

        public FakeConsumerResolver(IConsumerToMessageTypes consumerToMessageTypes)
        {
            m_consumerToMessageTypes = consumerToMessageTypes;
        }

        public IEnumerable<IConsumer> Resolve(Type messageType)
        {
            List<IConsumer> supportedConsumers;

            return consumers.TryGetValue(messageType, out supportedConsumers)
                       ? supportedConsumers
                       : Enumerable.Empty<IConsumer>();
        }

        public void Release(IEnumerable<IConsumer> consumers)
        {
            
        }

        public void Register(IConsumer consumer)
        {
            foreach (var supportedMessageType in m_consumerToMessageTypes.Get(consumer))
            {
                consumers.AddOrUpdate(supportedMessageType,
                                      new List<IConsumer>(),
                                      (type, list) =>
                                      {
                                          list.Add(consumer);
                                          return list;
                                      });
            }
        }
    }
}