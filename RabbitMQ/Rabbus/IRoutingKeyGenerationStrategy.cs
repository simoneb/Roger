using System;

namespace Rabbus
{
    public interface IRoutingKeyGenerationStrategy
    {
        string GetRoutingKey(Type messageType);
        string GetRoutingKey<T>() where T : class;
    }
}