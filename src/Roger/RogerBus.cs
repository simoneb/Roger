using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Roger.Internal;
using Roger.Internal.Impl;
using Roger.Messages;
using Roger.Utilities;

namespace Roger
{
    /// <summary>
    /// Main entry point of the library
    /// </summary>
    public class RogerBus : IRabbitBus, 
                            IReceive<ConnectionEstablished>, 
                            IReceive<ConnectionGracefulShutdown>,
                            IReceive<ConnectionAttemptFailed>, 
                            IReceive<ConnectionUnexpectedShutdown>
    {
        private readonly IReliableConnection connection;
        private readonly IConsumingProcess consumer;
        private readonly IPublishingProcess publisher;
        private readonly ILog log = LogManager.GetCurrentClassLogger();
        private readonly ITimer reconnectionTimer;
        private readonly MessageFilterCollection filters = new MessageFilterCollection();
        private readonly PublishModuleCollection publishModules = new PublishModuleCollection();
        private int disposed;
        private readonly Aggregator aggregator;
        private int started;

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
            aggregator = new Aggregator();
            reconnectionTimer = new SystemThreadingTimer();
            connection = new ReliableConnection(connectionFactory, reconnectionTimer, aggregator);
            
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

            Filters.Add(new ResequencingDeduplicationFilter());

            consumer = new DefaultConsumingProcess(idGenerator,
                                                   exchangeResolver,
                                                   serializer,
                                                   new DefaultMessageTypeResolver(), 
                                                   consumerContainer,
                                                   Filters,
                                                   queueFactory,
                                                   new AlwaysSuccessConsumerInvoker(), 
                                                   options,
                                                   aggregator);

            publisher = new QueueingPublishingProcess(idGenerator,
                                                      sequenceGenerator,
                                                      exchangeResolver,
                                                      serializer,
                                                      new DefaultMessageTypeResolver(),
                                                      publishModules,
                                                      aggregator);


            aggregator.Subscribe(this);
        }

        public event Action Started = delegate { };
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

        internal IConsumingProcess Consumer
        {
            get { return consumer; }
        }

        public void Start()
        {
            StartAsync().Wait();
        }

        public Task<IRabbitBus> StartAsync()
        {
            if(Interlocked.CompareExchange(ref started, 1, 0) == 1)
            {
                var task = new Task<IRabbitBus>(() => this);
                task.RunSynchronously();
                return task;
            }

            log.Debug("Starting bus");

            publisher.Start();

            return Task.Factory.StartNew(() =>
            {
                connection.Connect();
                return (IRabbitBus)this;
            });
        }

        public void Publish(object message, bool persistent = true, bool sequence = true)
        {
            publisher.Publish(message, persistent, sequence);
        }

        public void Request(object message, Action<BasicReturn> basicReturnCallback = null, bool persistent = true, bool sequence = false)
        {
            publisher.Request(message, basicReturnCallback, persistent, sequence);
        }

        public void Send(RogerEndpoint endpoint, object message, Action<BasicReturn> basicReturnCallback = null, bool persistent = true, bool sequence = false)
        {
            publisher.Send(endpoint, message, basicReturnCallback, persistent, sequence);
        }

        public void PublishMandatory(object message, Action<BasicReturn> basicReturnCallback = null, bool persistent = true, bool sequence = true)
        {
            publisher.PublishMandatory(message, basicReturnCallback, persistent, sequence);
        }

        public void Reply(object message, Action<BasicReturn> basicReturnCallback = null, bool persistent = true, bool sequence = false)
        {
            publisher.Reply(message, CurrentMessage, basicReturnCallback, persistent, sequence);
        }

        public IDisposable AddInstanceSubscription(IConsumer instanceConsumer)
        {
            return consumer.AddInstanceSubscription(instanceConsumer);
        }

        public void Consume(object message)
        {
            consumer.Consume(message);
        }

        void IReceive<ConnectionEstablished>.Receive(ConnectionEstablished message)
        {
            log.Debug("Bus started");
            Started();
        }

        void IReceive<ConnectionAttemptFailed>.Receive(ConnectionAttemptFailed message)
        {
            log.Debug("Bus interrupted");
            Interrupted();
        }

        void IReceive<ConnectionGracefulShutdown>.Receive(ConnectionGracefulShutdown message)
        {
            log.Debug("Bus Stopped");
            Stopped();
        }

        void IReceive<ConnectionUnexpectedShutdown>.Receive(ConnectionUnexpectedShutdown message)
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