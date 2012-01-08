using System;

namespace Roger.Internal
{
    internal interface IRoutingKeyResolver
    {
        string Resolve(Type messageType);
        string Resolve<T>() where T : class;
    }
}