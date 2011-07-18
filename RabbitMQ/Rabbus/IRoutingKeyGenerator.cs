using System;

namespace Rabbus
{
    public interface IRoutingKeyGenerator
    {
        string Generate(Type messageType);
        string Generate<T>() where T : class;
    }
}