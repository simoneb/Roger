using System;

namespace Rabbus.TypeNames
{
    public interface ITypeResolver
    {
        string GenerateTypeName<T>();
        string GenerateTypeName(Type type);
        Type ResolveType(string typeName);
    }
}