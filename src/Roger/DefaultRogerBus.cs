using System;
using System.Collections.Generic;
using System.Threading;
using RabbitMQ.Client;
using Roger.Internal;
using Roger.Internal.Impl;
using Roger.Utilities;

namespace Roger
{
    /// <summary>
    /// Main entry point of the library
    /// </summary>
    public class DefaultRogerBus : IRabbitBus
    {
        private readonly IReliableConnection connection;
        private readonly IConsumingProcess consumingProcess;
        private readonly IRogerLog log;
        private readonly IPublishingProcess publishingProcess;
        private int disposed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionFactory"></param>
        /// <param name="consumerResolver"></param>
        /// <param name="supportedMessageTypesResolver"></param>
        /// <param name="exchangeResolver"></param>
        /// <param name="routingKeyResolver"></param>
        /// <param name="serializer"></param>
        /// <param name="log"></param>
        /// <param name="idGenerator"></param>
        /// <param name="sequenceGenerator"></param>
        /// <param name="messageFilters"></param>
        /// <param name="noLocal">Configuress whether messages published by the bus should be received by consumers active on the same instance of the bus</param>
        public DefaultRogerBus(IConnectionFactory connectionFactory,
                                IConsumerResolver consumerResolver = null,
                                ISupportedMessageTypesResolver supportedMessageTypesResolver = null,
                                IExchangeResolver exchangeResolver = null,
                                IRoutingKeyResolver routingKeyResolver = null,
                                IMessageSerializer serializer = null,
                                IRogerLog log = null,
                                IIdGenerator idGenerator = null,
                                ISequenceGenerator sequenceGenerator = null,
                                IEnumerable<IMessageFilter> messageFilters = null,
                                bool noLocal = false)
        {
            consumerResolver = consumerResolver.Or(Default.ConsumerResolver);
            supportedMessageTypesResolver = supportedMessageTypesResolver.Or(Default.SupportedMessageTypesResolver);
            exchangeResolver = exchangeResolver.Or(Default.ExchangeResolver);
            routingKeyResolver = routingKeyResolver.Or(Default.RoutingKeyResolver);
            serializer = serializer.Or(Default.Serializer);
            idGenerator = idGenerator.Or(Default.IdGenerator);
            sequenceGenerator = sequenceGenerator.Or(Default.SequenceGenerator);
            messageFilters = messageFilters.Or(Default.Filters).ConcatIf(noLocal, new NoLocalFilter(() => LocalEndpoint));
            this.log = log.Or(Default.Log);

            connection = new ReliableConnection(connectionFactory, this.log);

            publishingProcess = new QueueingPublishingProcess(connection,
                                                              idGenerator,
                                                              sequenceGenerator,
                                                              exchangeResolver,
                                                              routingKeyResolver,
                                                              serializer,
                                                              Default.TypeResolver,
                                                              this.log,
                                                              () => LocalEndpoint,
                                                              TimeSpan.FromSeconds(10));

            consumingProcess = new DefaultConsumingProcess(connection,
                                                           idGenerator,
                                                           exchangeResolver,
                                                           routingKeyResolver,
                                                           serializer,
                                                           Default.TypeResolver, 
                                                           consumerResolver,
                                                           Default.Reflection, 
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

        public RogerEndpoint LocalEndpoint
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

        public void Send(RogerEndpoint endpoint, object message, Action<BasicReturn> basicReturnCallback = null)
        {
            publishingProcess.Send(endpoint, message, basicReturnCallback);
        }

        public void PublishMandatory(object message, Action<BasicReturn> basicReturnCallback = null)
        {
            publishingProcess.PublishMandatory(message, basicReturnCallback);
        }

        public void Reply(object message, Action<BasicReturn> basicReturnCallback = null)
        {
            publishingProcess.Reply(message, CurrentMessage, basicReturnCallback);
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