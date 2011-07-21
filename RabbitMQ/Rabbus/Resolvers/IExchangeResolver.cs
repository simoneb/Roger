using System;

namespace Rabbus.Resolvers
{
    public interface IExchangeResolver
    {
        string Resolve(Type messageType);
    }
}