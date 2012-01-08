using System;

namespace Roger.Internal
{
    internal interface IBindingKeyResolver
    {
        string Resolve(Type messageType);
    }
}