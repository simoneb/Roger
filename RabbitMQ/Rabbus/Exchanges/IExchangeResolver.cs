using System;

namespace Rabbus.Exchanges
{
    public interface IExchangeResolver
    {
        string Resolve(Type messageType);
    }
}