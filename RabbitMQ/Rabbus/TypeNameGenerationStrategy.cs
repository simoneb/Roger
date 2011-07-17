namespace Rabbus
{
    public class TypeNameGenerationStrategy : ITypeNameGenerationStrategy
    {
        public string GetName<T>()
        {
            var assemblyQualifiedName = typeof (T).AssemblyQualifiedName;
            return assemblyQualifiedName.Substring(0, assemblyQualifiedName.IndexOf(", Version="));
        }
    }
}