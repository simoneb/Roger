using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ManagedShovel
{
    public class ManagedShovel
    {
        private readonly ManagedShovelConfiguration configuration;
        private IConnection inboundConnection;
        private IConnection outboundConnection;
        private readonly ConcurrentDictionary<ulong, ulong> toBeConfirmed = new ConcurrentDictionary<ulong, ulong>();
        private LastCreatedQueueProxy inboundModel;
        private Thread thread;
        private readonly IEnumerator<string> sources;
        private readonly IEnumerator<string> destinations;

        internal ManagedShovel(ManagedShovelConfiguration configuration)
        {
            this.configuration = configuration;
            sources = new RollingEnumerator<string>(configuration.Sources);
            destinations = new RollingEnumerator<string>(configuration.Destinations);
        }

        public static FromDescriptor From(string broker, params string[] brokers)
        {
            var configuration = new ManagedShovelConfiguration {Sources = new[] {broker}.Concat(brokers).ToArray()};

            return new FromDescriptor(configuration);
        }

        internal void Start()
        {
            thread = new Thread(DoStart) {IsBackground = true};
            thread.Start();
        }

        private static void TryClose(IConnection connection)
        {
            if (connection == null) return;

            try
            {
                connection.Close();
            }
            catch
            {}
        }

        private void DoStart()
        {
            while (true)
            {
                Console.WriteLine("Starting...");

                try
                {
                    sources.MoveNext();

                    Console.WriteLine("Connecting inbound connection {0}", sources.Current);

                    inboundConnection = new ConnectionFactory {Uri = sources.Current}.CreateConnection();
                    inboundModel = new LastCreatedQueueProxy(inboundConnection.CreateModel());

                    inboundConnection.ConnectionShutdown += InboundConnectionShutdown;

                    foreach (var declaration in configuration.SourceDeclarations)
                        declaration(inboundModel);

                    var queue = configuration.LastCreatedQueue ? inboundModel.LastCreatedQueue : configuration.Queue;

                    if (string.IsNullOrWhiteSpace(queue))
                        throw new InvalidOperationException("Shovel queue is not defined");

                    if (configuration.PrefetchCount > 0)
                        inboundModel.BasicQos(0, configuration.PrefetchCount, false);

                    destinations.MoveNext();

                    Console.WriteLine("Connecting outbound connection {0}", destinations.Current);

                    outboundConnection = new ConnectionFactory {Uri = destinations.Current}.CreateConnection();
                    var outboundModel = outboundConnection.CreateModel();

                    outboundConnection.ConnectionShutdown += OutboundConnectionShutdown;

                    foreach (var declaration in configuration.DestinationDeclarations)
                        declaration(outboundModel);

                    if (configuration.AckMode == AckMode.OnConfirm)
                    {
                        outboundModel.ConfirmSelect();
                        outboundModel.BasicAcks += OutboundModelOnBasicAcks;
                    }

                    var consumer = new QueueingBasicConsumer(inboundModel);

                    inboundModel.BasicConsume(queue, configuration.AckMode == AckMode.NoAck, consumer);

                    Console.WriteLine("Started");

                    foreach (var delivery in consumer.Queue.Cast<BasicDeliverEventArgs>())
                    {
                        if (delivery.BasicProperties.Headers == null)
                            delivery.BasicProperties.Headers = new Hashtable();

                        var hops = delivery.BasicProperties.Headers["X-ManagedShovel-Hops"];

                        if(/*configuration.MaxHops > 0 && */ // uncomment to allow MaxHops = 0 enable messages to flow around forever
                            hops != null && BitConverter.ToInt32((byte[]) hops, 0) >= configuration.MaxHops)
                        {
                            Console.WriteLine("Ignoring incoming message as MaxHops reached");
                            inboundModel.BasicAck(delivery.DeliveryTag, false);
                            continue;
                        }

                        foreach (var action in configuration.PublishProperties)
                            action(delivery.BasicProperties);

                        if (configuration.AckMode == AckMode.OnConfirm)
                            toBeConfirmed.TryAdd(outboundModel.NextPublishSeqNo, delivery.DeliveryTag);
                     
                        if(hops == null)
                            delivery.BasicProperties.Headers["X-ManagedShovel-Hops"] = BitConverter.GetBytes(1);
                        else
                            delivery.BasicProperties.Headers["X-ManagedShovel-Hops"] = BitConverter.GetBytes(BitConverter.ToInt32((byte[]) hops, 0) + 1);

                        outboundModel.BasicPublish(configuration.PublishFields.Item1 ?? delivery.Exchange,
                                                   configuration.PublishFields.Item2 ?? delivery.RoutingKey,
                                                   delivery.BasicProperties,
                                                   delivery.Body);

                        if (configuration.AckMode == AckMode.OnPublish)
                            inboundModel.BasicAck(delivery.DeliveryTag, false);
                    }

                    if (inboundConnection.CloseReason.Initiator == ShutdownInitiator.Application ||
                        outboundConnection.CloseReason.Initiator == ShutdownInitiator.Application)
                    {
                        Console.WriteLine("Stopping");
                        break;
                    }

                    Console.WriteLine("No more elements in queue, continuing");
                    Console.WriteLine("Cleaning up before restarting in {0}", configuration.ReconnectDelay);

                    CleanupAndWait();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: {0}", new[]{e.Message});
                    Console.WriteLine("Cleaning up before restarting in {0}", configuration.ReconnectDelay);

                    CleanupAndWait();
                }
            }
        }

        private void CleanupAndWait()
        {
            Cleanup();

            if (configuration.ReconnectDelay > TimeSpan.Zero)
                Thread.Sleep(configuration.ReconnectDelay);
        }

        private void Cleanup()
        {
            TryClose(inboundConnection);
            TryClose(outboundConnection);
            toBeConfirmed.Clear();
        }

        private void OutboundConnectionShutdown(IConnection connection, ShutdownEventArgs reason)
        {
            Console.WriteLine("Inbound connection \"{0}\" shutdown", connection);
        }

        private void InboundConnectionShutdown(IConnection connection, ShutdownEventArgs reason)
        {
            Console.WriteLine("Outbound connection \"{0}\" shutdown", connection);
        }

        private void OutboundModelOnBasicAcks(IModel model, BasicAckEventArgs args)
        {
            ulong tag;

            if(toBeConfirmed.TryRemove(args.DeliveryTag, out tag))
                inboundModel.BasicAck(tag, false);
        }

        public void Stop()
        {
            Cleanup();

            thread.Join(100);

            Console.WriteLine("Stopped");
        }
    }
}