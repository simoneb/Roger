using System;

namespace Roger.Internal
{
    internal interface ITypeResolver
    {
        string Unresolve(Type type);
        Type Resolve(string typeName);
    }
}