using System;

namespace Rabbus.Resolvers
{
    public class DefaultRoutingKeyResolver : IRoutingKeyResolver
    {
        public string Resolve<T>() where T : class
        {
            return Resolve(typeof(T));
        }

        public string Resolve(Type messageType)
        {
            return messageType.FullName;
        }
    }
}