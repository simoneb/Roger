using System.Collections.Generic;
using RabbitMQ.Client;

namespace Roger
{
    /// <summary>
    /// Implementors receive a chance to intercept message arrivals and filter them
    /// </summary>
    public interface IMessageFilter
    {
        /// <summary>
        /// Chained with other filters to implement a message filtering pipeline
        /// </summary>
        /// <param name="input">The incoming messages, as an infinite enumerable</param>
        /// <param name="model">The model providing the messages</param>
        /// <returns>The messages to be passed on to following filters up until the final consumer</returns>
        /// <remarks>
        /// Messages which are filtered out should be ackowledged to avoid receiving them again
        /// </remarks>
        IEnumerable<CurrentMessageInformation> Filter(IEnumerable<CurrentMessageInformation> input, IModel model);
    }
}