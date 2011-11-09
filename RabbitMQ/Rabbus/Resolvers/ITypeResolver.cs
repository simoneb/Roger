using System;

namespace Rabbus.Resolvers
{
    public interface ITypeResolver
    {
        /// <summary>
        /// Provides the string representation of a type string representation
        /// </summary>
        /// <typeparam name="T">The input type</typeparam>
        /// <returns>The string representation of the type</returns>
        string Unresolve<T>();

        /// <summary>
        /// Provides the string representation of a type string representation
        /// </summary>
        /// <param name="type">The input type</param>
        /// <returns>The string representation of the type</returns>
        string Unresolve(Type type);

        /// <summary>
        /// Interprets the type from its string representation
        /// </summary>
        /// <param name="typeName">The string representation of the type</param>
        /// <returns>The type corresponding to the string representation</returns>
        Type ResolveType(string typeName);
    }
}