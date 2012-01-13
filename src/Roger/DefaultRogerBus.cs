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
        private readonly SystemThreadingTimer publisherConfirmsTimer;
        private readonly CompositePublishModule publishModule;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionFactory"></param>
        /// <param name="consumerContainer"></param>
        /// <param name="exchangeResolver"></param>
        /// <param name="serializer"></param>
        /// <param name="idGenerator"></param>
        /// <param name="sequenceGenerator"></param>
        /// <param name="messageFilters"></param>
        /// <param name="noLocal">Configuress whether messages published by the bus should be received by consumers active on the same instance of the bus</param>
        /// <param name="log"></param>
        public DefaultRogerBus(IConnectionFactory connectionFactory,
                               IConsumerContainer consumerContainer = null,
                               IExchangeResolver exchangeResolver = null,
                               IMessageSerializer serializer = null,
                               IIdGenerator idGenerator = null,
                               ISequenceGenerator sequenceGenerator = null,
                               IEnumerable<IMessageFilter> messageFilters = null,
                               bool noLocal = false,
                               IRogerLog log = null)
        {
            consumerContainer = consumerContainer.Or(Default.ConsumerContainer);
            exchangeResolver = exchangeResolver.Or(Default.ExchangeResolver);
            serializer = serializer.Or(Default.Serializer);
            idGenerator = idGenerator.Or(Default.IdGenerator);
            sequenceGenerator = sequenceGenerator.Or(Default.SequenceGenerator);
            messageFilters = messageFilters.Or(Default.Filters);
            this.log = log.Or(Default.Log);

            connection = new ReliableConnection(connectionFactory, this.log);

            publisherConfirmsTimer = new SystemThreadingTimer(TimeSpan.FromSeconds(1));
            publishModule = new CompositePublishModule(new PublisherConfirmsModule(publisherConfirmsTimer, log, TimeSpan.FromSeconds(2)),
                                                       new BasicReturnModule(log));

            // TODO: order here is important because both of the two guys below subscribe to
            // connection established events, but the publisher cannot start publish unless
            // the consumer has created the endpoint already
            consumingProcess = new DefaultConsumingProcess(connection,
                                                           idGenerator,
                                                           exchangeResolver,
                                                           serializer,
                                                           Default.TypeResolver, 
                                                           consumerContainer,
                                                           Default.Reflection, 
                                                           messageFilters,
                                                           this.log,
                                                           new DefaultQueueFactory(), 
                                                           noLocal);

            publishingProcess = new QueueingPublishingProcess(connection,
                                                              idGenerator,
                                                              sequenceGenerator,
                                                              exchangeResolver,
                                                              serializer,
                                                              Default.TypeResolver,
                                                              this.log,
                                                              () => LocalEndpoint,
                                                              publishModule);

            connection.ConnectionEstabilished += ConnectionEstabilished;
            connection.ConnectionAttemptFailed += ConnectionAttemptFailed;
            connection.UnexpectedShutdown += ConnectionUnexpectedShutdown;
        }

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

        public void Publish(object message, bool persistent = true)
        {
            publishingProcess.Publish(message, persistent);
        }

        public void Request(object message, Action<BasicReturn> basicReturnCallback = null, bool persistent = true)
        {
            publishingProcess.Request(message, basicReturnCallback, persistent);
        }

        public void Send(RogerEndpoint endpoint, object message, Action<BasicReturn> basicReturnCallback = null, bool persistent = true)
        {
            publishingProcess.Send(endpoint, message, basicReturnCallback, persistent);
        }

        public void PublishMandatory(object message, Action<BasicReturn> basicReturnCallback = null, bool persistent = true)
        {
            publishingProcess.PublishMandatory(message, basicReturnCallback, persistent);
        }

        public void Reply(object message, Action<BasicReturn> basicReturnCallback = null, bool persistent = true)
        {
            publishingProcess.Reply(message, CurrentMessage, basicReturnCallback, persistent);
        }

        public void Consume(object message)
        {
            consumingProcess.Consume(message);
        }

        private void ConnectionEstabilished()
        {
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
            consumingProcess.Dispose();
            publisherConfirmsTimer.Dispose();
            publishModule.Dispose();
            connection.Dispose();
        }
    }
}