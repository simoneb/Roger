using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Roger.Internal.Impl
{
    internal class QueueingPublishingProcess : IPublishingProcess
    {
        private readonly IReliableConnection connection;
        private readonly IRogerLog log;
        private int disposed;
        private Task publishTask;
        private readonly BlockingCollection<IDeliveryCommandFactory> publishingQueue = new BlockingCollection<IDeliveryCommandFactory>();
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        private readonly ManualResetEventSlim publishEnabled = new ManualResetEventSlim(false);
        private IModel publishModel;
        private readonly IIdGenerator idGenerator;
        private readonly ISequenceGenerator sequenceGenerator;
        private readonly IExchangeResolver exchangeResolver;
        private readonly IRoutingKeyResolver routingKeyResolver;
        private readonly IMessageSerializer serializer;
        private readonly ITypeResolver typeResolver;
        private readonly Func<RogerEndpoint> currentLocalEndpoint;
        private readonly IPublishModule modules;

        internal QueueingPublishingProcess(IReliableConnection connection,
                                           IIdGenerator idGenerator,
                                           ISequenceGenerator sequenceGenerator,
                                           IExchangeResolver exchangeResolver,
                                           IMessageSerializer serializer,
                                           ITypeResolver typeResolver,
                                           IRogerLog log,
                                           Func<RogerEndpoint> currentLocalEndpoint,
                                           IPublishModule modules)
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
            this.modules = modules;

            connection.ConnectionEstabilished += ConnectionOnConnectionEstabilished;
            connection.UnexpectedShutdown += ConnectionOnUnexpectedShutdown;

            modules.Initialize(this);
        }

        private void ConnectionOnConnectionEstabilished()
        {
            publishModel = connection.CreateModel();
            modules.BeforePublishEnabled(publishModel);

            EnablePublishing();
        }

        private void EnablePublishing()
        {
            publishEnabled.Set();
            log.Debug("Publishing is enabled");
        }

        private void ConnectionOnUnexpectedShutdown(ShutdownEventArgs shutdownEventArgs)
        {
            DisablePublishing();

            modules.AfterPublishDisabled();
        }

        private void DisablePublishing()
        {
            log.Warn("Disabling publishing due to unexpected connection shutdown");
            publishEnabled.Reset();
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

                        log.Debug("Executing publish action");

                        try
                        {
                            command.Execute(publishModel, currentLocalEndpoint(), modules);
                        }
                        /* 
                         * we may experience a newtork problem even before the connection notifies its own shutdown
                         * but it's safer not to disable publishing to avoid the risk of deadlocking
                         * Instead we catch the exception and hopefully will republish these messages
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

        public void Process(IDeliveryCommandFactory factory)
        {
            Enqueue(factory);
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

        public void Publish(object message, bool persistent)
        {
            var messageType = message.GetType();

            Enqueue(new PublishCommandFactory(messageType,
                                              Exchange(messageType),
                                              RoutingKey(messageType),
                                              Serialize(message), 
                                              persistent));
        }

        public void Request(object message, Action<BasicReturn> basicReturnCallback, bool persistent)
        {
            var messageType = message.GetType();

            Enqueue(new RequestCommandFactory(messageType,
                                              Exchange(messageType),
                                              RoutingKey(messageType),
                                              Serialize(message),
                                              persistent,
                                              basicReturnCallback));
        }

        public void Send(RogerEndpoint recipient, object message, Action<BasicReturn> basicReturnCallback, bool persistent)
        {
            var messageType = message.GetType();

            Enqueue(new SendCommandFactory(messageType,
                                           Exchange(messageType),
                                           recipient,
                                           Serialize(message),
                                           basicReturnCallback, 
                                           persistent));
        }

        public void PublishMandatory(object message, Action<BasicReturn> basicReturnCallback, bool persistent)
        {
            var messageType = message.GetType();

            Enqueue(new PublishMandatoryCommandFactory(messageType,
                                                       Exchange(messageType),
                                                       RoutingKey(messageType),
                                                       Serialize(message),
                                                       basicReturnCallback, 
                                                       persistent));
        }

        public void Reply(object message, CurrentMessageInformation currentMessage, Action<BasicReturn> basicReturnCallback, bool persistent = true)
        {
            EnsureRequestContext(currentMessage);
            ValidateReplyMessage(message);

            Enqueue(new ReplyCommandFactory(message.GetType(),
                                            Exchange(currentMessage.MessageType),
                                            currentMessage,
                                            Serialize(message),
                                            basicReturnCallback,
                                            persistent));
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
    }
}