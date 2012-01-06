using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;

namespace Roger.Internal.Impl
{
    internal class ResequencingFilter : IMessageFilter
    {
        private readonly ConcurrentDictionary<RogerEndpoint, uint> nextSequence = new ConcurrentDictionary<RogerEndpoint, uint>();
        private readonly ConcurrentDictionary<RogerEndpoint, SortedDictionary<uint, CurrentMessageInformation>> pending = new ConcurrentDictionary<RogerEndpoint, SortedDictionary<uint, CurrentMessageInformation>>();

        public IEnumerable<CurrentMessageInformation> Filter(IEnumerable<CurrentMessageInformation> input, IModel model)
        {
            foreach (var element in input)
            {
                var sequence = BitConverter.ToUInt32((byte[])element.Headers[Headers.Sequence], 0);
                var endpoint = element.Endpoint;

                if (!nextSequence.ContainsKey(endpoint) || sequence == nextSequence[endpoint])
                {
                    nextSequence[endpoint] = sequence + 1;
                    yield return element;

                    foreach (var p in ProcessPending(sequence, endpoint))
                        yield return p;
                }
                else if (sequence > nextSequence[endpoint]) // out of order
                {
                    pending[endpoint].Add(sequence, element);
                }
            }
        }

        private IEnumerable<CurrentMessageInformation> ProcessPending(uint currentSequence, RogerEndpoint endpoint)
        {
            var currentPending = pending.GetOrAdd(endpoint, SortedDictionary).Where(s => s.Key > currentSequence).ToArray();

            foreach (var p in currentPending)
                pending[endpoint].Remove(p.Key);

            return currentPending.Select(p => p.Value);
        }

        private static SortedDictionary<uint, CurrentMessageInformation> SortedDictionary
        {
            get { return new SortedDictionary<uint, CurrentMessageInformation>(); }
        }
    }
}