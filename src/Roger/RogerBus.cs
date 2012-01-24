using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using RabbitMQ.Client;
using Roger.Internal;
using Roger.Internal.Impl;
using Roger.Utilities;

namespace Roger
{
    /// <summary>
    /// Main entry point of the library
    /// </summary>
    public class RogerBus : IRabbitBus
    {
        private readonly IReliableConnection connection;
        private readonly IConsumingProcess consumer;
        private readonly IPublishingProcess publisher;
        private readonly ILog log = LogManager.GetCurrentClassLogger();
        private readonly ITimer reconnectionTimer;
        private readonly MessageFilterCollection filters = new MessageFilterCollection();
        private readonly PublishModuleCollection publishModules = new PublishModuleCollection();
        private int disposed;


        /// <summary>
        /// Default library entry point
        /// </summary>
        /// <param name="connectionFactory"></param>
        /// <param name="consumerContainer"></param>
        /// <param name="exchangeResolver"></param>
        /// <param name="serializer"></param>
        /// <param name="idGenerator"></param>
        /// <param name="sequenceGenerator"></param>
        /// <param name="options"> </param>
        public RogerBus(IConnectionFactory connectionFactory,
                        IConsumerContainer consumerContainer = null,
                        IExchangeResolver exchangeResolver = null,
                        IMessageSerializer serializer = null,
                        IIdGenerator idGenerator = null,
                        ISequenceGenerator sequenceGenerator = null,
                        RogerOptions options = null)
        {
            reconnectionTimer = new SystemThreadingTimer();
            connection = new ReliableConnection(connectionFactory, reconnectionTimer);
            
            consumerContainer = consumerContainer.Or(new EmptyConsumerContainer());
            exchangeResolver = exchangeResolver.Or(new AttributeExchangeResolver());
            serializer = serializer.Or(new ProtoBufNetSerializer());
            idGenerator = idGenerator.Or(new RandomIdGenerator());
            sequenceGenerator = sequenceGenerator.Or(new ByMessageHirarchyRootSequenceGenerator());
            options = options.Or(new RogerOptions());
            
            publishModules.Add(new BasicReturnModule());

            if(options.UsePublisherConfirms)
                publishModules.AddFirst(new PublisherConfirmsModule(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)));

            var queueFactory = new DefaultQueueFactory(true, 
                                                       false, 
                                                       false, 
                                                       options.QueueUnusedTimeout,
                                                       options.MessageTimeToLiveOnQueue, 
                                                       options.QueueName);

            if (options.DeduplicationAndResequencing)
                Filters.Add(new ResequencingDeduplicationFilter());

            // TODO: order here is important because both of the two guys below subscribe to
            // connection established events, but the publisher cannot start publish unless
            // the consumer has created the endpoint already
            consumer = new DefaultConsumingProcess(connection,
                                                   idGenerator,
                                                   exchangeResolver,
                                                   serializer,
                                                   new DefaultTypeResolver(), 
                                                   consumerContainer,
                                                   Filters,
                                                   queueFactory,
                                                   new AlwaysSuccessConsumerInvoker(), 
                                                   options);

            publisher = new QueueingPublishingProcess(connection,
                                                      idGenerator,
                                                      sequenceGenerator,
                                                      exchangeResolver,
                                                      serializer,
                                                      new DefaultTypeResolver(), 
                                                      () => LocalEndpoint,
                                                      publishModules);

            connection.ConnectionEstabilished += ConnectionEstabilished;
            connection.ConnectionAttemptFailed += ConnectionAttemptFailed;
            connection.UnexpectedShutdown += ConnectionUnexpectedShutdown;
            connection.GracefulShutdown += ConnectionGracefulShutdown;
        }

        private void ConnectionGracefulShutdown()
        {
            log.Debug("Bus Stopped");
            Stopped();
        }

        public event Action Stopped = delegate {  };

        public event Action Interrupted = delegate { };

        public CurrentMessageInformation CurrentMessage
        {
            get { return consumer.CurrentMessage; }
        }

        public RogerEndpoint LocalEndpoint
        {
            get { return consumer.Endpoint; }
        }

        public TimeSpan ConnectionAttemptInterval
        {
            get { return connection.ConnectionAttemptInterval; }
        }

        public MessageFilterCollection Filters
        {
            get { return filters; }
        }

        public void Start()
        {
            StartAsync().Wait();
        }

        public Task<IRabbitBus> StartAsync()
        {
            log.Debug("Starting bus");

            publisher.Start();

            return Task.Factory.StartNew(() =>
            {
                connection.Connect();
                return (IRabbitBus)this;
            });
        }

        public IDisposable AddInstanceSubscription(IConsumer instanceConsumer)
        {
            return consumer.AddInstanceSubscription(instanceConsumer);
        }

        public void Publish(object message, bool persistent = true)
        {
            publisher.Publish(message, persistent);
        }

        public void Request(object message, Action<BasicReturn> basicReturnCallback = null, bool persistent = true)
        {
            publisher.Request(message, basicReturnCallback, persistent);
        }

        public void Send(RogerEndpoint endpoint, object message, Action<BasicReturn> basicReturnCallback = null, bool persistent = true)
        {
            publisher.Send(endpoint, message, basicReturnCallback, persistent);
        }

        public void PublishMandatory(object message, Action<BasicReturn> basicReturnCallback = null, bool persistent = true)
        {
            publisher.PublishMandatory(message, basicReturnCallback, persistent);
        }

        public void Reply(object message, Action<BasicReturn> basicReturnCallback = null, bool persistent = true)
        {
            publisher.Reply(message, CurrentMessage, basicReturnCallback, persistent);
        }

        public void Consume(object message)
        {
            consumer.Consume(message);
        }

        private void ConnectionEstabilished()
        {
            log.Debug("Bus started");
            Started();
        }

        public event Action Started = delegate { };

        private void ConnectionAttemptFailed()
        {
            log.Debug("Bus interrupted");
            Interrupted();
        }

        private void ConnectionUnexpectedShutdown(ShutdownEventArgs obj)
        {
            log.Debug("Bus interrupted");
            Interrupted();
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) == 1)
                return;

            log.Debug("Disposing bus");

            publisher.Dispose();
            consumer.Dispose();
            reconnectionTimer.Dispose();
            publishModules.Dispose();
            connection.Dispose();
        }
    }
}