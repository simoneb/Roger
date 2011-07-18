using System;

namespace Rabbus
{
    public class DefaultRoutingKeyGenerator : IRoutingKeyGenerator
    {
        public string Generate<T>() where T : class
        {
            return Generate(typeof(T));
        }

        public string Generate(Type messageType)
        {
            return messageType.FullName;
        }
    }
}