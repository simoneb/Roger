using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using RabbitMQ.Client;
using Roger.Utilities;

namespace Roger.Internal.Impl
{
    internal class ResequencingDeduplicationFilter : IMessageFilter
    {
        private readonly ConcurrentDictionary<RogerEndpoint, uint> nextSequences = new ConcurrentDictionary<RogerEndpoint, uint>();
        private readonly ConcurrentDictionary<RogerEndpoint, SortedDictionary<uint, CurrentMessageInformation>> pendingMessages = new ConcurrentDictionary<RogerEndpoint, SortedDictionary<uint, CurrentMessageInformation>>();
        private readonly ILog log = LogManager.GetCurrentClassLogger();

        public IEnumerable<CurrentMessageInformation> Filter(IEnumerable<CurrentMessageInformation> input, IModel model)
        {
            foreach (var message in input)
            {
                var receivedSequence = BitConverter.ToUInt32((byte[])message.Headers[Headers.Sequence], 0);
                var endpoint = message.Endpoint;

                if (Unknown(endpoint) || CorrectSequence(endpoint, receivedSequence))
                {
                    nextSequences[endpoint] = receivedSequence + 1;
                    log.DebugFormat("Correct sequence {0} on endpoint {1}", receivedSequence, endpoint);
                    yield return message;

                    foreach (var p in Filter(Pending(endpoint, receivedSequence), model))
                        yield return p;
                }
                else if (Unordered(endpoint, receivedSequence))
                {
                    log.WarnFormat("Unexpected sequence {0} on endpoint {1}. Was expecting {2}", receivedSequence, endpoint, nextSequences[endpoint]);
                    pendingMessages[endpoint].Add(receivedSequence, message);
                }
                else
                {
                    log.DebugFormat("Filtering out (and acking) message sequence {0} - id {1} on endpoint {2} as already processed", 
                                    receivedSequence, 
                                    message.MessageId, 
                                    endpoint);

                    model.BasicAck(message.DeliveryTag, false);
                }
            }
        }

        private bool Unordered(RogerEndpoint endpoint, uint sequence)
        {
            return sequence > nextSequences[endpoint];
        }

        private bool CorrectSequence(RogerEndpoint endpoint, uint sequence)
        {
            return sequence == nextSequences[endpoint];
        }

        private bool Unknown(RogerEndpoint endpoint)
        {
            return !nextSequences.ContainsKey(endpoint);
        }

        private IEnumerable<CurrentMessageInformation> Pending(RogerEndpoint endpoint, uint receivedSequence)
        {
            var toProcess = pendingMessages.GetOrAdd(endpoint, SortedDictionary)
                                           .Where(p => p.Key, (p, c) => c == p + 1, receivedSequence)
                                           .ToArray();

            if(toProcess.Any())
                log.DebugFormat("Processing {0} out of {1} pending messages for endpoint {2}", toProcess.Length, pendingMessages[endpoint].Count, endpoint);

            foreach (var p in toProcess)
                pendingMessages[endpoint].Remove(p.Key);

            return toProcess.Select(p => p.Value);
        }

        private static SortedDictionary<uint, CurrentMessageInformation> SortedDictionary
        {
            get { return new SortedDictionary<uint, CurrentMessageInformation>(); }
        }
    }
}