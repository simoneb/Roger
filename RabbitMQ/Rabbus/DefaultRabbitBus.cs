using System;
using System.Collections.Generic;
using System.Threading;
using RabbitMQ.Client;
using Rabbus.Connection;
using Rabbus.Consuming;
using Rabbus.Errors;
using Rabbus.GuidGeneration;
using Rabbus.Logging;
using Rabbus.Publishing;
using Rabbus.Reflection;
using Rabbus.Resolvers;
using Rabbus.Sequencing;
using Rabbus.Serialization;
using Rabbus.Utilities;

namespace Rabbus
{
    public class DefaultRabbitBus : IRabbitBus
    {
        private readonly IReliableConnection connection;
        private readonly IConsumingProcess consumingProcess;
        private readonly IRabbusLog log;
        private readonly IPublishingProcess publishingProcess;
        private int disposed;

        public DefaultRabbitBus(IConnectionFactory connectionFactory,
                                IConsumerResolver consumerResolver = null,
                                ITypeResolver typeResolver = null,
                                ISupportedMessageTypesResolver supportedMessageTypesResolver = null,
                                IExchangeResolver exchangeResolver = null,
                                IRoutingKeyResolver routingKeyResolver = null,
                                IMessageSerializer serializer = null,
                                IReflection reflection = null,
                                IRabbusLog log = null,
                                IGuidGenerator guidGenerator = null,
                                ISequenceGenerator sequenceGenerator = null,
                                IEnumerable<IMessageFilter> messageFilters = null)
        {
            consumerResolver = consumerResolver.Or(Default.ConsumerResolver);
            typeResolver = typeResolver.Or(Default.TypeResolver);
            supportedMessageTypesResolver = supportedMessageTypesResolver.Or(Default.SupportedMessageTypesResolver);
            exchangeResolver = exchangeResolver.Or(Default.ExchangeResolver);
            reflection = reflection.Or(Default.Reflection);
            routingKeyResolver = routingKeyResolver.Or(Default.RoutingKeyResolver);
            serializer = serializer.Or(Default.Serializer);
            guidGenerator = guidGenerator.Or(Default.GuidGenerator);
            sequenceGenerator = sequenceGenerator.Or(Default.SequenceGenerator);
            messageFilters = messageFilters.Or(Default.Filters);
            this.log = log.Or(Default.Log);

            connection = new ReliableConnection(connectionFactory, this.log);

            publishingProcess = new QueueingPublishingProcess(connection,
                                                              guidGenerator,
                                                              sequenceGenerator,
                                                              exchangeResolver,
                                                              routingKeyResolver,
                                                              serializer,
                                                              typeResolver,
                                                              this.log,
                                                              () => LocalEndpoint);

            consumingProcess = new DefaultConsumingProcess(connection,
                                                           guidGenerator,
                                                           exchangeResolver,
                                                           routingKeyResolver,
                                                           serializer,
                                                           typeResolver,
                                                           consumerResolver,
                                                           reflection,
                                                           supportedMessageTypesResolver,
                                                           messageFilters,
                                                           this.log);

            connection.ConnectionEstabilished += ConnectionEstabilished;
            connection.ConnectionAttemptFailed += ConnectionAttemptFailed;
            connection.UnexpectedShutdown += ConnectionUnexpectedShutdown;
        }

        public event Action Started = delegate { };
        public event Action ConnectionFailure = delegate { };

        public CurrentMessageInformation CurrentMessage
        {
            get { return consumingProcess.CurrentMessage; }
        }

        public RabbusEndpoint LocalEndpoint
        {
            get { return consumingProcess.Endpoint; }
        }

        public TimeSpan ConnectionAttemptInterval
        {
            get { return connection.ConnectionAttemptInterval; }
        }

        public void Start()
        {
            log.Debug("Starting bus");

            connection.Connect();
            publishingProcess.Start();
        }

        public IDisposable AddInstanceSubscription(IConsumer instanceConsumer)
        {
            return consumingProcess.AddInstanceSubscription(instanceConsumer);
        }

        public void Publish(object message)
        {
            publishingProcess.Publish(message);
        }

        public void Request(object message, Action<BasicReturn> basicReturnCallback = null)
        {
            publishingProcess.Request(message, basicReturnCallback);
        }

        public void Send(RabbusEndpoint endpoint, object message, Action<BasicReturn> basicReturnCallback = null)
        {
            publishingProcess.Send(endpoint, message, basicReturnCallback);
        }

        public void PublishMandatory(object message, Action<BasicReturn> basicReturnCallback = null)
        {
            publishingProcess.PublishMandatory(message, basicReturnCallback);
        }

        public void Reply(object message)
        {
            publishingProcess.Reply(message, CurrentMessage);
        }

        public void Consume(object message)
        {
            consumingProcess.Consume(message);
        }

        private void ConnectionEstabilished()
        {
            Started();
            log.Debug("Bus started");
        }

        private void ConnectionAttemptFailed()
        {
            ConnectionFailure();
        }

        private void ConnectionUnexpectedShutdown(ShutdownEventArgs obj)
        {
            ConnectionFailure();
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) == 1)
                return;

            log.Debug("Disposing bus");

            publishingProcess.Dispose();

            // TODO: beware that currently here order is important as consumer won't stop unless connection is closed
            connection.Dispose();
            consumingProcess.Dispose();
        }
    }
}