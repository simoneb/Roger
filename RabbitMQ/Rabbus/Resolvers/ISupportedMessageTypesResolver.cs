using System;
using System.Collections.Generic;

namespace Rabbus.Resolvers
{
    /// <summary>
    /// Defines the mapping between a consumer type and the messages it can consume
    /// </summary>
    public interface ISupportedMessageTypesResolver
    {
        /// <summary>
        /// Gets the message types that the supplied <paramref name="consumerType"/> can handle
        /// </summary>
        /// <param name="consumerType">The type of the consumer</param>
        /// <returns>The supported message types</returns>
        ISet<Type> Resolve(Type consumerType);
    }
}