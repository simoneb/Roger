using System;

namespace Rabbus.Resolvers
{
    public interface ITypeResolver
    {
        string Unresolve<T>();
        string Unresolve(Type type);
        Type ResolveType(string typeName);
    }
}