namespace Roger
{
    /// <summary>
    /// Marker interface
    /// </summary>
    public interface IConsumer
    {
    }

    public interface IConsumer<T> : IConsumer where T : class
    {
        void Consume(T message);
    }

    public interface IConsumer1<T> : IConsumer where T : class
    {
        void Consume(T message);
    }

    public interface IConsumer2<T> : IConsumer where T : class
    {
        void Consume(T message);
    }

    //public interface IConsumer<T1, T2> : IConsumer
    //    where T1 : class
    //    where T2 : class
    //{
    //    void Consume(T1 message);
    //    void Consume(T2 message);
    //}

    //public interface IConsumer<T1, T2, T3> : IConsumer
    //    where T1 : class
    //    where T2 : class
    //    where T3 : class
    //{
    //    void Consume(T1 message);
    //    void Consume(T2 message);
    //    void Consume(T3 message);
    //}
}