namespace Rabbus
{
    public interface ITypeNameGenerationStrategy
    {
        string GetName<T>();
    }
}