using System;

namespace Roger
{
    public interface IRoutingKeyResolver
    {
        string Resolve(Type messageType);
        string Resolve<T>() where T : class;
    }
}