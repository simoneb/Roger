using System;

namespace Rabbus.Resolvers
{
    public interface IRoutingKeyResolver
    {
        string Resolve(Type messageType);
        string Resolve<T>() where T : class;
    }
}