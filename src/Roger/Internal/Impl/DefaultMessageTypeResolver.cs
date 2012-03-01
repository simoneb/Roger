using System;
using Common.Logging;

namespace Roger.Internal.Impl
{
    internal class DefaultMessageTypeResolver : IMessageTypeResolver
    {
        private readonly ILog log = LogManager.GetCurrentClassLogger();

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
            catch (TypeLoadException e)
            {
                log.DebugFormat("Ignoring message with type name \"{0}\" because it cannot be loaded from its name", e, typeName);
                type = null;
                return false;
            }
        }
    }
}