using System;

namespace Rabbus
{
    public class TypeNameGenerator : ITypeNameGenerator
    {
        public string GetName<T>()
        {
            return GetName(typeof(T));
        }

        public string GetName(Type type)
        {
            var fullName = type.AssemblyQualifiedName;
            return fullName.Substring(0, fullName.IndexOf(", Version="));
        }
    }
}