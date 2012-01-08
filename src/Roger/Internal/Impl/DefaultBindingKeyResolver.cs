using System;

namespace Roger.Internal.Impl
{
    internal class DefaultBindingKeyResolver : IBindingKeyResolver
    {
        private readonly IRoutingKeyResolver routingKeyResolver;

        public DefaultBindingKeyResolver(IRoutingKeyResolver routingKeyResolver)
        {
            this.routingKeyResolver = routingKeyResolver;
        }

        public string Resolve(Type messageType)
        {
            return routingKeyResolver.Resolve(messageType) + ".#";
        }
    }
}