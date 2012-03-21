using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Roger.Utilities;

namespace Roger.Internal.Impl
{
    internal class ResequencingDeduplicationFilter : IMessageFilter
    {
        private readonly ConcurrentDictionary<SequenceKey, uint> nextSequences = new ConcurrentDictionary<SequenceKey, uint>();
        private readonly ConcurrentDictionary<SequenceKey, SortedDictionary<uint, CurrentMessageInformation>> pendingMessages = new ConcurrentDictionary<SequenceKey, SortedDictionary<uint, CurrentMessageInformation>>();
        private readonly ILog log = LogManager.GetCurrentClassLogger();

        public IEnumerable<CurrentMessageInformation> Filter(IEnumerable<CurrentMessageInformation> input, IModel model)
        {
            foreach (var message in input)
            {
                if (message.Headers == null)
                {
                    log.Warn("Received message with no headers, perhaps it's coming from an unexpected source?");
                    TryAckFilteredMessage(model, message);
                }
                else if(!message.Headers.ContainsKey(Headers.Sequence))
                    yield return message;
                else
                {
                    var receivedSequence = BitConverter.ToUInt32((byte[]) message.Headers[Headers.Sequence], 0);
                    var sequenceKey = new SequenceKey(message.Endpoint, message.MessageType);

                    if (Unknown(sequenceKey) || CorrectSequence(sequenceKey, receivedSequence))
                    {
                        nextSequences[sequenceKey] = receivedSequence + 1;
                        log.TraceFormat("Correct sequence {0} for key {1}", receivedSequence, sequenceKey);
                        yield return message;

                        foreach (var p in Filter(Pending(sequenceKey, receivedSequence), model))
                            yield return p;
                    }
                    else if (Unordered(sequenceKey, receivedSequence))
                    {
                        log.WarnFormat("Unexpected sequence {0} for key {1}. Was expecting {2}", receivedSequence,
                                       sequenceKey, nextSequences[sequenceKey]);
                        pendingMessages[sequenceKey].Add(receivedSequence, message);
                    }
                    else
                    {
                        log.DebugFormat(
                            "Filtering out (and acking) message sequence {0} - id {1} for key {2} as already processed",
                            receivedSequence,
                            message.MessageId,
                            sequenceKey);

                        TryAckFilteredMessage(model, message);
                    }
                }
            }
        }

        private void TryAckFilteredMessage(IModel model, CurrentMessageInformation message)
        {
            try
            {
                model.BasicAck(message.DeliveryTag, false);
            }
            catch (AlreadyClosedException e)
            {
                log.Info("Could not ack filtered-out message because model was already closed", e);
            }
            catch (Exception e)
            {
                log.Warn("Could not ack filtered-out message for unknown cause", e);
            }
        }

        internal struct SequenceKey
        {
            public readonly RogerEndpoint Endpoint;
            public readonly Type MessageType;

            public SequenceKey(RogerEndpoint endpoint, Type messageType)
            {
                Endpoint = endpoint;
                MessageType = messageType.HierarchyRoot();
            }

            public override string ToString()
            {
                return string.Format("{0}/{1}", Endpoint, MessageType);
            }
        }

        private bool Unordered(SequenceKey key, uint sequence)
        {
            return sequence > nextSequences[key];
        }

        private bool CorrectSequence(SequenceKey key, uint sequence)
        {
            return sequence == nextSequences[key];
        }

        private bool Unknown(SequenceKey key)
        {
            return !nextSequences.ContainsKey(key);
        }

        private IEnumerable<CurrentMessageInformation> Pending(SequenceKey key, uint receivedSequence)
        {
            var toProcess = pendingMessages.GetOrAdd(key, SortedDictionary)
                                           .Where(p => p.Key, (p, c) => c == p + 1, receivedSequence)
                                           .ToArray();

            if(toProcess.Any())
                log.DebugFormat("Processing {0} out of {1} pending messages for key {2}", toProcess.Length, pendingMessages[key].Count, key);

            foreach (var p in toProcess)
                pendingMessages[key].Remove(p.Key);

            return toProcess.Select(p => p.Value);
        }

        private static SortedDictionary<uint, CurrentMessageInformation> SortedDictionary
        {
            get { return new SortedDictionary<uint, CurrentMessageInformation>(); }
        }
    }
}