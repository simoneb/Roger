using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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
            var routingKey = new StringBuilder(messageType.Namespace);

            var baseType = messageType;
            var stack = new Stack<string>();

            while (baseType != null && baseType != typeof(object))
            {
                stack.Push(baseType.Name);
                baseType = baseType.BaseType;
            }

            stack.Aggregate(routingKey, (b, s) => b.Append("." + s));

            if(routingKey.Length > 255)
                throw new InvalidOperationException("Routing key should be shorter than 256 characters");

            return routingKey.ToString();
        }
    }
}