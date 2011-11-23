using System;
using Rabbus.Resolvers;

namespace Rabbus.Chat.Client
{
    public class StaticExchangeResolver : IExchangeResolver
    {
        private readonly string exchangeName;

        public StaticExchangeResolver(string exchangeName)
        {
            this.exchangeName = exchangeName;
        }

        public string Resolve(Type messageType)
        {
            return exchangeName;
        }
    }
}