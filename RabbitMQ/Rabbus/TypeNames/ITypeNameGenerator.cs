using System;

namespace Rabbus.TypeNames
{
    public interface ITypeNameGenerator
    {
        string Generate<T>();
        string Generate(Type type);
    }
}