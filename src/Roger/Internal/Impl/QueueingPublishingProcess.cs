using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Linq;
using RabbitMQ.Client.Exceptions;

namespace Roger.Internal.Impl
{
    internal class QueueingPublishingProcess : IPublishingProcess
    {
        private readonly IReliableConnection connection;
        private readonly IBasicReturnHandler basicReturnHandler;
        private readonly IRogerLog log;
        private int disposed;
        private Task publishTask;
        private readonly BlockingCollection<IDeliveryCommandFactory> publishingQueue = new BlockingCollection<IDeliveryCommandFactory>();
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        private readonly ManualResetEventSlim publishEnabled = new ManualResetEventSlim(false);
        private IModel publishModel;
        private readonly ConcurrentDictionary<ulong, IUnconfirmedCommandFactory> unconfirmedCommands = new ConcurrentDictionary<ulong, IUnconfirmedCommandFactory>();
        private readonly IIdGenerator idGenerator;
        private readonly ISequenceGenerator sequenceGenerator;
        private readonly IExchangeResolver exchangeResolver;
        private readonly IRoutingKeyResolver routingKeyResolver;
        private readonly IMessageSerializer serializer;
        private readonly ITypeResolver typeResolver;
        private readonly Func<RogerEndpoint> currentLocalEndpoint;
        private readonly TimeSpan republishUnconfirmedMessagesThreshold;

        internal QueueingPublishingProcess(IReliableConnection connection,
                                           IIdGenerator idGenerator,
                                           ISequenceGenerator sequenceGenerator,
                                           IExchangeResolver exchangeResolver,
                                           IMessageSerializer serializer,
                                           ITypeResolver typeResolver,
                                           IRogerLog log,
                                           Func<RogerEndpoint> currentLocalEndpoint,
                                           TimeSpan republishUnconfirmedMessagesThreshold)
        {
            this.connection = connection;
            this.log = log;
            this.idGenerator = idGenerator;
            this.sequenceGenerator = sequenceGenerator;
            this.exchangeResolver = exchangeResolver;
            routingKeyResolver = Default.RoutingKeyResolver;
            this.serializer = serializer;
            this.typeResolver = typeResolver;
            this.currentLocalEndpoint = currentLocalEndpoint;
            this.republishUnconfirmedMessagesThreshold = republishUnconfirmedMessagesThreshold;

            basicReturnHandler = new DefaultBasicReturnHandler(this.log);

            connection.ConnectionEstabilished += ConnectionOnConnectionEstabilished;
            connection.UnexpectedShutdown += ConnectionOnUnexpectedShutdown;
        }

        private void ConnectionOnUnexpectedShutdown(ShutdownEventArgs shutdownEventArgs)
        {
            DisablePublishing();
        }

        private void DisablePublishing()
        {
            log.Warn("Disabling publishing due to unexpected connection shutdown");
            publishEnabled.Reset();
        }

        private void ConnectionOnConnectionEstabilished()
        {
            CreatePublishModel();
            EnablePublishing();
        }

        private void CreatePublishModel()
        {
            publishModel = connection.CreateModel();

            publishModel.BasicAcks += PublishModelOnBasicAcks;
            publishModel.BasicNacks += PublishModelOnBasicNacks;
            publishModel.BasicReturn += PublishModelOnBasicReturn;
            publishModel.ConfirmSelect();
        }

        private void EnablePublishing()
        {
            publishEnabled.Set();
            log.Debug("Publishing is enabled");
        }

        private void PublishModelOnBasicAcks(IModel model, BasicAckEventArgs args)
        {
            IUnconfirmedCommandFactory _;

            if (args.Multiple)
            {
                log.DebugFormat("Broker confirmed all deliveries up to and including {0}", args.DeliveryTag);

                var toRemove = unconfirmedCommands.Keys.Where(tag => tag <= args.DeliveryTag).ToArray();

                foreach (var tag in toRemove)
                    unconfirmedCommands.TryRemove(tag, out _);
            }
            else
            {
                log.DebugFormat("Broker confirmed delivery {0}", args.DeliveryTag);
                unconfirmedCommands.TryRemove(args.DeliveryTag, out _);
            }

            log.DebugFormat("Deliveries yet to be confirmed: {0}", unconfirmedCommands.Count);
        }

        private void PublishModelOnBasicNacks(IModel model, BasicNackEventArgs args)
        {
            if (args.Multiple)
                log.WarnFormat("Broker nacked all deliveries up to and including {0}", args.DeliveryTag);
            else
                log.WarnFormat("Broker nacked delivery {0}", args.DeliveryTag);
        }

        private void PublishModelOnBasicReturn(IModel model, BasicReturnEventArgs args)
        {
            // beware, this is called on the RabbitMQ client connection thread, we should not block
            log.DebugFormat("Model issued a basic return for message {{we can do better here}} with reply {0} - {1}", args.ReplyCode, args.ReplyText);
            basicReturnHandler.Handle(new BasicReturn(new RogerGuid(args.BasicProperties.MessageId), args.ReplyCode, args.ReplyText));
        }

        public void Start()
        {
            publishTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    foreach (var factory in publishingQueue.GetConsumingEnumerable(tokenSource.Token))
                    {
                        try
                        {
                            publishEnabled.Wait(tokenSource.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            // operation canceled while waiting for publish to be enabled, just break out of the loop
                            break;
                        }
                        
                        var command = factory.Create(publishModel, idGenerator, typeResolver, serializer, sequenceGenerator);

                        unconfirmedCommands.TryAdd(publishModel.NextPublishSeqNo, new UnconfirmedCommandFactory(command, republishUnconfirmedMessagesThreshold));

                        log.Debug("Executing publish action");

                        try
                        {
                            command.Execute(publishModel, currentLocalEndpoint(), basicReturnHandler);
                        }
                        /* 
                         * we may experience a newtork problem even before the connection notifies its own shutdown
                         * but it's safer not to disable publishing to avoid the risk of deadlocking
                         * Instead we catch the exception and hopefully republish these messages
                         */
                        catch (AlreadyClosedException e)
                        {
                            log.ErrorFormat("Model was already closed when trying to publish on it\r\n{0}", e);
                        }
                        catch (IOException e)
                        {
                            log.ErrorFormat("IO error when trying to publish\r\n{0}", e);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // operation canceled while iterating over the queue, do nothing and let task complete
                }
                catch (ObjectDisposedException)
                {
                    log.Error("Publishing queue was disposed while iterating over it, this is not supposed to be happening");
                }
            }, TaskCreationOptions.LongRunning);
        }

        private void Enqueue(IDeliveryCommandFactory factory)
        {
            try
            {
                log.Debug("Enqueuing publish action");
                publishingQueue.Add(factory);
            }
            catch (ObjectDisposedException)
            {
                log.Error("Could not enqueue message for publishing as publishing queue has been disposed of already");
            }
            catch (InvalidOperationException e)
            {
                log.ErrorFormat("Could not enqueue message for publishing: {0}", e);
            }
        }

        public void Publish(object message)
        {
            var messageType = message.GetType();

            Enqueue(new PublishCommandFactory(messageType,
                                              Exchange(messageType),
                                              RoutingKey(messageType),
                                              Serialize(message)));
        }

        public void Request(object message, Action<BasicReturn> basicReturnCallback)
        {
            var messageType = message.GetType();

            Enqueue(new RequestCommandFactory(messageType,
                                              Exchange(messageType),
                                              RoutingKey(messageType),
                                              Serialize(message), 
                                              basicReturnCallback));
        }

        public void Send(RogerEndpoint recipient, object message, Action<BasicReturn> basicReturnCallback)
        {
            var messageType = message.GetType();

            Enqueue(new SendCommandFactory(messageType,
                                           Exchange(messageType),
                                           recipient,
                                           Serialize(message),
                                           basicReturnCallback));
        }

        public void PublishMandatory(object message, Action<BasicReturn> basicReturnCallback)
        {
            var messageType = message.GetType();

            Enqueue(new PublishMandatoryCommandFactory(messageType,
                                                       Exchange(messageType),
                                                       RoutingKey(messageType),
                                                       Serialize(message),
                                                       basicReturnCallback));
        }

        public void Reply(object message, CurrentMessageInformation currentMessage, Action<BasicReturn> basicReturnCallback)
        {
            EnsureRequestContext(currentMessage);
            ValidateReplyMessage(message);

            Enqueue(new ReplyCommandFactory(message.GetType(),
                                            Exchange(currentMessage.MessageType),
                                            currentMessage,
                                            Serialize(message),
                                            basicReturnCallback));
        }

        private void EnsureRequestContext(CurrentMessageInformation currentMessage)
        {
            if (currentMessage == null ||
                currentMessage.Endpoint.IsEmpty ||
                currentMessage.CorrelationId.IsEmpty)
            {
                log.Error("Reply method called out of the context of a message handling request");
                throw new InvalidOperationException(ErrorMessages.ReplyInvokedOutOfRequestContext);
            }
        }

        private void ValidateReplyMessage(object message)
        {
            if (!message.GetType().IsDefined(typeof(RogerReplyAttribute), false))
            {
                log.Error("Reply method called with a reply message not decorated woth the right attribute");
                throw new InvalidOperationException(ErrorMessages.ReplyMessageNotAReply);
            }
        }

        private string Exchange(Type messageType)
        {
            return exchangeResolver.Resolve(messageType);
        }

        private string RoutingKey(Type messageType)
        {
            return routingKeyResolver.Resolve(messageType);
        }

        private byte[] Serialize(object message)
        {
            return serializer.Serialize(message);
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) == 1)
                return;

            publishingQueue.CompleteAdding();
            tokenSource.Cancel();
            publishTask.Wait();

            publishingQueue.Dispose();
            tokenSource.Dispose();
            publishTask.Dispose();
        }

        internal void ProcessUnconfirmed()
        {
            var toProcess = unconfirmedCommands.Where(p => p.Value.CanExecute);

            foreach (var tp in toProcess)
            {
                IUnconfirmedCommandFactory publish;
                unconfirmedCommands.TryRemove(tp.Key, out publish);
                Enqueue(publish);         
            }
        }
    }
}