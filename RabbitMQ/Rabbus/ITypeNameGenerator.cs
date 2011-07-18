using System;

namespace Rabbus
{
    public interface ITypeNameGenerator
    {
        string Generate<T>();
        string Generate(Type type);
    }
}