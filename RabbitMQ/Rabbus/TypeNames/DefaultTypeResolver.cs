using System;

namespace Rabbus.TypeNames
{
    public class DefaultTypeResolver : ITypeResolver
    {
        public string GenerateTypeName<T>()
        {
            return GenerateTypeName(typeof(T));
        }

        public string GenerateTypeName(Type type)
        {
            var fullName = type.AssemblyQualifiedName;
            return fullName.Substring(0, fullName.IndexOf(", Version="));
        }

        public Type ResolveType(string typeName)
        {
            return Type.GetType(typeName, true);
        }
    }
}