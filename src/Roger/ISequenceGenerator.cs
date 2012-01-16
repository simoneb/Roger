using System;

namespace Roger
{
    /// <summary>
    /// Generates sequence numbers for messages
    /// </summary>
    public interface ISequenceGenerator
    {
        uint Next(Type messageType);
    }
}