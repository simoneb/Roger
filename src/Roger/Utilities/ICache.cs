namespace Roger.Utilities
{
    public interface ICache<T>
    {
        bool TryAdd(T key);
        void PauseEvictions();
        void ResumeEvictions();
    }
}