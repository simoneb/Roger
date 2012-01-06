namespace Roger.Utilities
{
    public interface ICache<T>
    {
        bool TryAdd(T key);
    }
}