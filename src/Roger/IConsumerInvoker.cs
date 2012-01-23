using System.Collections.Generic;

namespace Roger
{
    /// <summary>
    /// Invokes consumers with a message they can consume
    /// </summary>
    public interface IConsumerInvoker
    {
        /// <summary>
        /// Invokes the consumers
        /// </summary>
        /// <param name="consumers">The consumers eligible to receive the message</param>
        /// <param name="message">The message to be consumed</param>
        /// <returns><c>True</c> if the message is considered to have been consumed, <c>False</c> otherwise</returns>
        bool Invoke(IEnumerable<IConsumer> consumers, CurrentMessageInformation message);
    }
}