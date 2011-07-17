using System;

namespace Rabbus
{
    public interface IRoutingKeyGenerator
    {
        string GetRoutingKey(Type messageType);
        string GetRoutingKey<T>() where T : class;
    }
}