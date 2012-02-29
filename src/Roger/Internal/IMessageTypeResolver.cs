using System;

namespace Roger.Internal
{
    internal interface IMessageTypeResolver
    {
        string Unresolve(Type type);
        bool TryResolve(string typeName, out Type type);
    }
}