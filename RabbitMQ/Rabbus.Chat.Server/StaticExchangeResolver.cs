using System;

namespace Rabbus.Chat.Server
{
    internal class StaticExchangeResolver : IExchangeResolver
    {
        private readonly string exchange;

        public StaticExchangeResolver(string exchange)
        {
            this.exchange = exchange;
        }

        public string Resolve(Type messageType)
        {
            return exchange;
        }
    }
}