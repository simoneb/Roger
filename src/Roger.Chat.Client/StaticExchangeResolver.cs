using System;

namespace Roger.Chat.Client
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

        public bool IsReply(Type messageType)
        {
            return false;
        }
    }
}