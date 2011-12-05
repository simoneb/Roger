using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Rabbus.Connection;
using Rabbus.Errors;
using Rabbus.GuidGeneration;
using Rabbus.Logging;
using Rabbus.Resolvers;
using Rabbus.Returns;
using Rabbus.Serialization;

namespace Rabbus.Publishing
{
    internal class QueueingPublisher : IPublisher
    {
        private readonly IReliableConnection connection;
        private readonly IBasicReturnHandler basicReturnHandler;
        private readonly IRabbusLog log;
        private int disposed;
        private Task publishTask;
        private readonly BlockingCollection<Action<IModel>> publishingQueue = new BlockingCollection<Action<IModel>>();
        private readonly CancellationTokenSource publishCancellationSource = new CancellationTokenSource();
        private readonly ManualResetEventSlim publishEnabled = new ManualResetEventSlim(false);
        private IModel publishModel;
        private readonly ConcurrentDictionary<IModel, SortedSet<ulong>> unconfirmedPublishes = new ConcurrentDictionary<IModel, SortedSet<ulong>>();
        private readonly IGuidGenerator guidGenerator;
        private readonly IExchangeResolver exchangeResolver;
        private readonly IRoutingKeyResolver routingKeyResolver;
        private readonly IMessageSerializer serializer;
        private readonly ITypeResolver typeResolver;
        private readonly Func<RabbusEndpoint> localEndpoint;

        internal QueueingPublisher(IReliableConnection connection, IRabbusLog log, IGuidGenerator guidGenerator, IExchangeResolver exchangeResolver, IRoutingKeyResolver routingKeyResolver, IMessageSerializer serializer, ITypeResolver typeResolver, Func<RabbusEndpoint> localEndpoint)
        {
            this.connection = connection;
            this.log = log;
            this.guidGenerator = guidGenerator;
            this.exchangeResolver = exchangeResolver;
            this.routingKeyResolver = routingKeyResolver;
            this.serializer = serializer;
            this.typeResolver = typeResolver;
            this.localEndpoint = localEndpoint;

            basicReturnHandler = new DefaultBasicReturnHandler(this.log);

            connection.ConnectionEstabilished += ConnectionOnConnectionEstabilished;
            connection.UnexpectedShutdown += ConnectionOnUnexpectedShutdown;
        }

        private void ConnectionOnUnexpectedShutdown(ShutdownEventArgs shutdownEventArgs)
        {
            log.Warn("Disabling publishing due to unexpected connection shutdown");
            publishEnabled.Reset();
        }

        private void ConnectionOnConnectionEstabilished()
        {
            CreatePublishModel();
            EnablePublishing();
        }

        private void EnablePublishing()
        {
            publishEnabled.Set();
            log.Debug("Publishing is enabled");
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
            publishTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    foreach (var publish in publishingQueue.GetConsumingEnumerable(publishCancellationSource.Token))
                    {
                        try
                        {
                            publishEnabled.Wait(publishCancellationSource.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            // operation canceled while waiting for publish to be enabled, just break out of the loop
                            break;
                        }
                        publish(publishModel);
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

        private void Enqueue(Action<IModel> action)
        {
            try
            {
                log.Debug("Enqueuing publish action");
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

        public void Publish(object message)
        {
            Enqueue(model =>
            {
                var messageType = message.GetType();
                var properties = CreateProperties(model, messageType);

                //ModelConfirms(PublishModel).Add(model.NextPublishSeqNo);

                model.BasicPublish(exchangeResolver.Resolve(messageType), routingKeyResolver.Resolve(messageType), properties, serializer.Serialize(message));
            });
        }

        public void Request(object message, Action<BasicReturn> basicReturnCallback)
        {
            Enqueue(model =>
            {
                var properties = CreateProperties(model, message.GetType());
                properties.CorrelationId = guidGenerator.Next();

                PublishMandatoryInternal(model, message, properties, basicReturnCallback);

                log.DebugFormat("Issued request with message {0}", message.GetType());
            });
        }

        public void Send(RabbusEndpoint endpoint, object message, Action<BasicReturn> basicReturnCallback)
        {
            Enqueue(model =>
            {
                var properties = CreateProperties(model, message.GetType());

                PublishMandatoryInternal(model, message, properties, basicReturnCallback, endpoint.Queue);
            });
        }

        public void PublishMandatory(object message, Action<BasicReturn> basicReturnCallback)
        {
            Enqueue(model =>
            {
                var properties = CreateProperties(model, message.GetType());

                PublishMandatoryInternal(model, message, properties, basicReturnCallback);
            });
        }

        public void Reply(object message, CurrentMessageInformation currentMessage)
        {
            EnsureRequestContext(currentMessage);
            ValidateReplyMessage(message);

            Enqueue(model =>
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
            if (!message.GetType().IsDefined(typeof(RabbusReplyAttribute), false))
            {
                log.Error("Reply method called with a reply message not decorated woth the right attribute");
                throw new InvalidOperationException(ErrorMessages.ReplyMessageNotAReply);
            }
        }

        private void EnsureRequestContext(CurrentMessageInformation currentMessage)
        {
            if (currentMessage == null ||
                currentMessage.Endpoint.IsEmpty() ||
                currentMessage.CorrelationId.IsEmpty)
            {
                log.Error("Reply method called out of the context of a message handling request");
                throw new InvalidOperationException(ErrorMessages.ReplyInvokedOutOfRequestContext);
            }
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
            if (basicReturnCallback != null)
                basicReturnHandler.Subscribe(new RabbusGuid(properties.MessageId), basicReturnCallback);

            model.BasicPublish(exchangeResolver.Resolve(message.GetType()),
                               routingKey,
                               true,
                               false,
                               properties,
                               serializer.Serialize(message));
        }

        private IBasicProperties CreateProperties(IModel model, Type messageType)
        {
            var properties = model.CreateBasicProperties();

            properties.MessageId = guidGenerator.Next();
            properties.Type = typeResolver.Unresolve(messageType);
            properties.ReplyTo = localEndpoint().Queue;
            properties.ContentType = serializer.ContentType;

            return properties;
        }
        

        private SortedSet<ulong> ModelConfirms(IModel model)
        {
            return unconfirmedPublishes.GetOrAdd(model, new SortedSet<ulong>());
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) == 1)
                return;

            publishingQueue.CompleteAdding();
            publishCancellationSource.Cancel();
            publishTask.Wait();

            publishingQueue.Dispose();
            publishCancellationSource.Dispose();
            publishTask.Dispose();
        }
    }
}