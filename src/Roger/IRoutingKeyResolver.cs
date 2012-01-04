using System;

namespace Rabbus
{
    public interface IRoutingKeyResolver
    {
        string Resolve(Type messageType);
        string Resolve<T>() where T : class;
    }
}