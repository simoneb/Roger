using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Rabbus.Errors;
using Rabbus.GuidGeneration;
using Rabbus.Logging;
using Rabbus.Reflection;
using Rabbus.Resolvers;
using Rabbus.Returns;
using Rabbus.Serialization;
using Rabbus.Utilities;

namespace Rabbus
{
    public class DefaultRabbitBus : IRabbitBus
    {
        private readonly IRoutingKeyResolver routingKeyResolver;
        private readonly ITypeResolver typeResolver;
        private readonly IMessageSerializer serializer;
        private readonly IReflection reflection;
        private readonly IConsumerResolver consumerResolver;
        private readonly ISupportedMessageTypesResolver supportedMessageTypesResolver;
        private readonly IExchangeResolver exchangeResolver;
        private readonly IRabbusLog log;
        private readonly IReliableConnection connection;
        private readonly IGuidGenerator guidGenerator;

        public event Action Started = delegate { };
        public event Action ConnectionFailure = delegate { };

        [ThreadStatic]
        private static CurrentMessageInformation _currentMessage;

        public CurrentMessageInformation CurrentMessage { get { return _currentMessage; } }
        public RabbusEndpoint LocalEndpoint { get; private set; }
        public TimeSpan ConnectionAttemptInterval { get { return connection.ConnectionAttemptInterval; } }

        private readonly ConcurrentDictionary<WeakReference, object> instanceConsumers = new ConcurrentDictionary<WeakReference, object>();
        private IModel receivingModel;
        private IModel publishModel;
        private bool disposed;
        private readonly IBasicReturnHandler basicReturnHandler;
        private QueueingBasicConsumer queueConsumer;
        private readonly ConcurrentDictionary<IModel, SortedSet<ulong>> unconfirmedPublishes = new ConcurrentDictionary<IModel, SortedSet<ulong>>();
        private readonly BlockingCollection<Action<IModel>> publishingQueue = new BlockingCollection<Action<IModel>>();
        private readonly ManualResetEvent publishingCompleted = new ManualResetEvent(false);
        private readonly CancellationTokenSource publishCancellationSource = new CancellationTokenSource();

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
            this.consumerResolver = consumerResolver.Or(Default.ConsumerResolver);
            this.typeResolver = typeResolver.Or(Default.TypeResolver);
            this.supportedMessageTypesResolver = supportedMessageTypesResolver.Or(Default.SupportedMessageTypesResolver);
            this.exchangeResolver = exchangeResolver.Or(Default.ExchangeResolver);
            this.reflection = reflection.Or(Default.Reflection);
            this.routingKeyResolver = routingKeyResolver.Or(Default.RoutingKeyResolver);
            this.serializer = serializer.Or(Default.Serializer);
            this.log = log.Or(Default.Log);
            this.guidGenerator = guidGenerator.Or(Default.GuidGenerator);

            basicReturnHandler = new DefaultBasicReturnHandler(this.log);
            connection = new ReliableConnection(connectionFactory, this.log, ConnectionEstabilished);

            connection.ConnectionAttemptFailed += HandleConnectionAttemptFailed;
            connection.UnexpectedShutdown += HandleConnectionUnexpectedShutdown;
        }

        private void HandleConnectionUnexpectedShutdown(ShutdownEventArgs obj)
        {
            ConnectionFailure();
        }

        private void HandleConnectionAttemptFailed()
        {
            ConnectionFailure();            
        }

        private void CreatePublishModel()
        {
            publishModel = connection.CreateModel();

            //publishModel.BasicAcks += PublishModelOnBasicAcks;
            publishModel.BasicReturn += PublishModelOnBasicReturn;
            //publishModel.ConfirmSelect();
        }

        private void PublishModelOnBasicAcks(IModel model, BasicAckEventArgs args)
        {
            if (args.Multiple)
            {
                log.DebugFormat("Broker confirmed all messages up to and including {0}", args.DeliveryTag);
                ModelConfirms(model).RemoveWhere(confirm => confirm <= args.DeliveryTag);
            }
            else
            {
                log.DebugFormat("Broker confirmed message {0}", args.DeliveryTag);
                ModelConfirms(model).Remove(args.DeliveryTag);
            }
        }

        private void PublishModelOnBasicReturn(IModel model, BasicReturnEventArgs args)
        {
            // beware, this is called on the RabbitMQ client connection thread, therefore we 
            // should use the model parameter rather than the ThreadLocal property. Also we should not block
            log.DebugFormat("Model issued a basic return for message {{we can do better here}} with reply {0} - {1}", args.ReplyCode, args.ReplyText);
            basicReturnHandler.Handle(new BasicReturn(new RabbusGuid(args.BasicProperties.MessageId), args.ReplyCode, args.ReplyText));
        }

        public void Start()
        {
            log.Debug("Starting bus");

            connection.Connect();

            StartPublishingLoop();
        }

        private void ConnectionEstabilished()
        {
            CreatePublishModel();
            receivingModel = connection.CreateModel();

            LocalEndpoint = new RabbusEndpoint(receivingModel.QueueDeclare("", false, true, false, null));

            CreateConsumer();
            ConsumeAsynchronously();

            Started();

            log.Debug("Bus started");
        }

        private void StartPublishingLoop()
        {
            Task.Factory.StartNew(() =>
            {
                // catch ODE
                foreach (var publish in publishingQueue.GetConsumingEnumerable(publishCancellationSource.Token))
                {
                    Debug.WriteLine("Publishing message");
                    publish(publishModel);
                }

                Debug.WriteLine("Exited publishing loop");

                publishingCompleted.Set();

                Debug.WriteLine("Set publishing gate");
            }, TaskCreationOptions.LongRunning);
        }

        private void CreateConsumer()
        {
            CreateBindings(consumerResolver.GetAllSupportedMessageTypes());

            queueConsumer = new QueueingBasicConsumer(receivingModel);
            receivingModel.BasicConsume(LocalEndpoint.Queue, false, queueConsumer);
        }

        private void CreateBindings(ISet<Type> messageTypes)
        {
            // Here we allow eventual duplicate bindings if this method is called multiple times which result
            // in queues being bound to the same exchange with the same arguments
            // http://www.rabbitmq.com/amqp-0-9-1-reference.html#queue.bind

            var allExchanges = new HashSet<string>();

            log.Debug("Performing pub/sub bindings");

            foreach (var messageType in messageTypes.ExceptReplies())
            {
                var exchange = exchangeResolver.Resolve(messageType);
                var routingKey = routingKeyResolver.Resolve(messageType);

                log.DebugFormat("Binding queue {0} to exchange {1} with routing key {2}", LocalEndpoint, exchange, routingKey);

                receivingModel.QueueBind(LocalEndpoint.Queue, exchange, routingKey);

                allExchanges.Add(exchange);
            }

            log.Debug("Performing private messages bindings");

            foreach (var exchange in allExchanges)
            {
                log.DebugFormat("Binding queue {0} to exchange {1} with quete name as routing key", LocalEndpoint, exchange);

                receivingModel.QueueBind(LocalEndpoint.Queue, exchange, LocalEndpoint.Queue);
            }
        }

        private void ConsumeAsynchronously()
        {
            Task.Factory.StartNew(ConsumeSynchronously, TaskCreationOptions.LongRunning);
        }

        private void ConsumeSynchronously()
        {
            foreach (var message in BlockingDequeue(queueConsumer.Queue))
            {
                SetCurrentMessageAndInvokeConsumers(message);
                queueConsumer.Model.BasicAck(message.DeliveryTag, false);
            }
        }

        private IEnumerable<CurrentMessageInformation> BlockingDequeue(IEnumerable queue)
        {
            return queue.OfType<BasicDeliverEventArgs>().Select(CreateMessage);
        }

        private CurrentMessageInformation CreateMessage(BasicDeliverEventArgs args)
        {
            var properties = args.BasicProperties;
            var messageType = typeResolver.ResolveType(properties.Type);

            return new CurrentMessageInformation
                   {
                       MessageId = new RabbusGuid(properties.MessageId),
                       MessageType = messageType,
                       Endpoint = new RabbusEndpoint(properties.ReplyTo),
                       CorrelationId = string.IsNullOrWhiteSpace(properties.CorrelationId) ? RabbusGuid.Empty : new RabbusGuid(properties.CorrelationId),
                       DeliveryTag = args.DeliveryTag,
                       Exchange = args.Exchange,
                       Body = serializer.Deserialize(messageType, args.Body)
                   };
        }

        private void SetCurrentMessageAndInvokeConsumers(CurrentMessageInformation message)
        {
            _currentMessage = message;

            var consumers = ResolveConsumers(_currentMessage.MessageType);

            var localInstanceConsumers = consumers.Item1.ToArray();
            var defaultConsumers = consumers.Item2.ToArray();

            log.DebugFormat("Found {0} standard consumers and {1} instance consumers for message {2}",
                            defaultConsumers.Length,
                            localInstanceConsumers.Length,
                            _currentMessage.MessageType);

            var allConsumers = localInstanceConsumers.Concat(defaultConsumers);

            foreach (var c in allConsumers)
            {
                log.DebugFormat("Invoking Consume method on consumer {0} for message {1}",
                                c.GetType(),
                                _currentMessage.MessageType);

                reflection.InvokeConsume(c, _currentMessage.Body);
            }

            consumerResolver.Release(defaultConsumers);
        }

        public IDisposable AddInstanceSubscription(IConsumer consumer)
        {
            var consumerReference = new WeakReference(consumer);
            instanceConsumers.TryAdd(consumerReference, null);

            CreateBindings(GetSupportedMessageTypes(consumer));

            // TODO: queue bindings are not removed, no problem unless we start adding too many instance subscriptions
            return RemoveInstanceConsumer(consumerReference);
        }

        private IDisposable RemoveInstanceConsumer(WeakReference consumerReference)
        {
            return new DisposableAction(() =>
            {
                object _;
                instanceConsumers.TryRemove(consumerReference, out _);
            });
        }

        private ISet<Type> GetSupportedMessageTypes(IConsumer consumer)
        {
            return supportedMessageTypesResolver.Resolve(consumer.GetType());
        }

        public void Publish(object message)
        {
            EnqueueDefaultPublish(message);
        }

        private void EnqueueDefaultPublish(object message)
        {
            EnqueuePublish(model =>
            {
                var messageType1 = message.GetType();
                var properties1 = CreateProperties(model, messageType1);

                //ModelConfirms(PublishModel).Add(model.NextPublishSeqNo);

                model.BasicPublish(exchangeResolver.Resolve(messageType1), routingKeyResolver.Resolve(messageType1), properties1, serializer.Serialize(message));
            });
        }

        private void EnqueuePublish(Action<IModel> action)
        {
            try
            {
                publishingQueue.Add(action);
            }
            catch (ObjectDisposedException)
            {
                log.Error("Cound not enqueue message for publishing as publishing queue has been disposed of already");
            }
            catch (InvalidOperationException e)
            {
                log.ErrorFormat("Cound not enqueue message for publishing: {0}", e);
            }
        }

        private SortedSet<ulong> ModelConfirms(IModel model)
        {
            return unconfirmedPublishes.GetOrAdd(model, new SortedSet<ulong>());
        }

        private IBasicProperties CreateProperties(IModel model, Type messageType)
        {
            var properties = model.CreateBasicProperties();

            properties.MessageId = guidGenerator.Next();
            properties.Type = typeResolver.Unresolve(messageType);
            properties.ReplyTo = LocalEndpoint.Queue;
            properties.ContentType = serializer.ContentType;
            
            return properties;
        }
        
        public void Request(object message, Action<BasicReturn> basicReturnCallback = null)
        {
            EnqueueRequest(message, basicReturnCallback);
        }

        private void EnqueueRequest(object message, Action<BasicReturn> basicReturnCallback)
        {
            EnqueuePublish(model =>
            {
                var properties = CreateProperties(model, message.GetType());
                properties.CorrelationId = guidGenerator.Next();

                PublishMandatoryInternal(model, message, properties, basicReturnCallback);

                log.DebugFormat("Issued request with message {0}", message.GetType());
            });
        }

        public void Send(RabbusEndpoint endpoint, object message, Action<BasicReturn> basicReturnCallback = null)
        {
            EnqueueSend(endpoint, message, basicReturnCallback);
        }

        private void EnqueueSend(RabbusEndpoint endpoint, object message, Action<BasicReturn> basicReturnCallback)
        {
            EnqueuePublish(model =>
            {
                var properties = CreateProperties(model, message.GetType());

                PublishMandatoryInternal(model, message, properties, basicReturnCallback, endpoint.Queue);
            });
        }

        public void PublishMandatory(object message, Action<BasicReturn> basicReturnCallback = null)
        {
            EnqueuePublish(model =>
            {
                var properties = CreateProperties(model, message.GetType());

                PublishMandatoryInternal(model, message, properties, basicReturnCallback);
            });
        }

        private void PublishMandatoryInternal(IModel model,
                                              object message,
                                              IBasicProperties properties,
                                              Action<BasicReturn> basicReturnCallback)
        {
            PublishMandatoryInternal(model, message, properties, basicReturnCallback, routingKeyResolver.Resolve(message.GetType()));
        }

        private void PublishMandatoryInternal(IModel model,
                                              object message,
                                              IBasicProperties properties,
                                              Action<BasicReturn> basicReturnCallback,
                                              string routingKey)
        {
            if(basicReturnCallback != null)
                basicReturnHandler.Subscribe(new RabbusGuid(properties.MessageId), basicReturnCallback);

            model.BasicPublish(exchangeResolver.Resolve(message.GetType()),
                               routingKey,
                               true,
                               false,
                               properties,
                               serializer.Serialize(message));
        }

        public void Reply(object message)
        {
            EnsureRequestContext();

            ValidateReplyMessage(message);

            var currentMessage = CurrentMessage;

            EnqueuePublish(model =>
            {
                var properties = CreateProperties(model, message.GetType());
                properties.CorrelationId = currentMessage.CorrelationId.ToString();

                // reply on the same exchange of the request message
                model.BasicPublish(exchangeResolver.Resolve(currentMessage.MessageType),
                                   currentMessage.Endpoint,
                                   properties,
                                   serializer.Serialize(message));
            });
        }

        private void ValidateReplyMessage(object message)
        {
            if (!message.GetType().IsDefined(typeof (RabbusReplyAttribute), false))
            {
                log.Error("Reply method called with a reply message not decorated woth the right attribute");
                throw new InvalidOperationException(ErrorMessages.ReplyMessageNotAReply);
            }
        }

        private void EnsureRequestContext()
        {
            if (CurrentMessage == null ||
                CurrentMessage.Endpoint.IsEmpty() ||
                CurrentMessage.CorrelationId.IsEmpty)
            {
                log.Error("Reply method called out of the context of a message handling request");
                throw new InvalidOperationException(ErrorMessages.ReplyInvokedOutOfRequestContext);
            }
        }

        private Tuple<IEnumerable<IConsumer>, IEnumerable<IConsumer>> ResolveConsumers(Type messageType)
        {
            return Tuple.Create(InstanceConsumers(messageType), consumerResolver.Resolve(messageType));
        }

        private IEnumerable<IConsumer> InstanceConsumers(Type messageType)
        {
            return instanceConsumers.Keys
                .Where(r => r.IsAlive)
                .Select(r => r.Target)
                .Cast<IConsumer>()
                .Where(c => GetSupportedMessageTypes(c).Contains(messageType));
        }

        public void Consume(object message)
        {
            SetCurrentMessageAndInvokeConsumers(new CurrentMessageInformation
            {
                MessageId = guidGenerator.Next(),
                Body = message,
                MessageType = message.GetType()
            });
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            publishingQueue.CompleteAdding();

            Debug.WriteLine("Called CompleteAdding");

            publishingCompleted.WaitOne();

            Debug.WriteLine("WatedOne on publishing completed gate");

            publishingQueue.Dispose();

            Debug.WriteLine("Disposed publishing queue");
            publishingCompleted.Dispose();

            log.Debug("Disposing bus");
            connection.Dispose();
        }
    }
}