using System;

namespace Rabbus.Resolvers
{
    public class DefaultTypeResolver : ITypeResolver
    {
        public string Unresolve<T>()
        {
            return Unresolve(typeof(T));
        }

        public string Unresolve(Type type)
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