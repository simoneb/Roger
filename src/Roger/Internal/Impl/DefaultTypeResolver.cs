using System;

namespace Roger.Internal.Impl
{
    internal class DefaultTypeResolver : ITypeResolver
    {
        public string Unresolve<T>()
        {
            return Unresolve(typeof(T));
        }

        public string Unresolve(Type type)
        {
            var fullName = type.AssemblyQualifiedName;
            return fullName.Substring(0, fullName.IndexOf(", Version=", StringComparison.Ordinal));
        }

        public Type Resolve(string typeName)
        {
            return Type.GetType(typeName, true);
        }
    }
}