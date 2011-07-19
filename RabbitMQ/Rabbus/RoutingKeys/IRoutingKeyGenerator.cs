using System;

namespace Rabbus.RoutingKeys
{
    public interface IRoutingKeyGenerator
    {
        string Generate(Type messageType);
        string Generate<T>() where T : class;
    }
}