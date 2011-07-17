using System;

namespace Rabbus
{
    public interface ITypeNameGenerator
    {
        string GetName<T>();
        string GetName(Type type);
    }
}