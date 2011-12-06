using System;
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
using Rabbus.Serialization;
using Rabbus.Utilities;

namespace Rabbus
{
    public class DefaultRabbitBus : IRabbitBus
    {
        private readonly IReliableConnection connection;
        private readonly IConsumingProcess consumer;
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
                                IGuidGenerator guidGenerator = null)
        {
            consumerResolver = consumerResolver.Or(Default.ConsumerResolver);
            typeResolver = typeResolver.Or(Default.TypeResolver);
            supportedMessageTypesResolver = supportedMessageTypesResolver.Or(Default.SupportedMessageTypesResolver);
            exchangeResolver = exchangeResolver.Or(Default.ExchangeResolver);
            reflection = reflection.Or(Default.Reflection);
            routingKeyResolver = routingKeyResolver.Or(Default.RoutingKeyResolver);
            serializer = serializer.Or(Default.Serializer);
            guidGenerator = guidGenerator.Or(Default.GuidGenerator);
            this.log = log.Or(Default.Log);

            connection = new ReliableConnection(connectionFactory, this.log);

            publishingProcess = new QueueingPublishingProcess(connection,
                                              guidGenerator,
                                              exchangeResolver,
                                              routingKeyResolver,
                                              serializer,
                                              typeResolver,
                                              this.log,
                                              () => LocalEndpoint);

            consumer = new DefaultConsumingProcess(connection,
                                           guidGenerator,
                                           exchangeResolver,
                                           routingKeyResolver,
                                           serializer,
                                           typeResolver,
                                           consumerResolver,
                                           reflection,
                                           supportedMessageTypesResolver,
                                           this.log);

            connection.ConnectionEstabilished += ConnectionEstabilished;
            connection.ConnectionAttemptFailed += ConnectionAttemptFailed;
            connection.UnexpectedShutdown += ConnectionUnexpectedShutdown;
        }

        public event Action Started = delegate { };
        public event Action ConnectionFailure = delegate { };

        public CurrentMessageInformation CurrentMessage
        {
            get { return consumer.CurrentMessage; }
        }

        public RabbusEndpoint LocalEndpoint
        {
            get { return consumer.Endpoint; }
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
            return consumer.AddInstanceSubscription(instanceConsumer);
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
            consumer.Consume(message);
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

            publishingProcess.Dispose();

            log.Debug("Disposing bus");
            connection.Dispose();
        }
    }
}