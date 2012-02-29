using System;

namespace Roger.Internal.Impl
{
    internal class DefaultMessageTypeResolver : IMessageTypeResolver
    {
        public string Unresolve(Type type)
        {
            var fullName = type.AssemblyQualifiedName;
            return fullName.Substring(0, fullName.IndexOf(", Version=", StringComparison.Ordinal));
        }

        public bool TryResolve(string typeName, out Type type)
        {
            try
            {
                type = Type.GetType(typeName, true);
                return true;
            }
            catch (TypeLoadException)
            {
                type = null;
                return false;
            }
        }
    }
}