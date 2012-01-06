using System;

namespace Roger.Internal.Impl
{
    internal class DefaultRoutingKeyResolver : IRoutingKeyResolver
    {
        public string Resolve<T>() where T : class
        {
            return Resolve(typeof(T));
        }

        public string Resolve(Type messageType)
        {
            var routingKey = messageType.FullName;

            if(routingKey.Length > 255)
                throw new InvalidOperationException("Routing key should be shorter than 256 characters");

            return routingKey;
        }
    }
}