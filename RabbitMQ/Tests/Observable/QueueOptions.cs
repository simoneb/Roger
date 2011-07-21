namespace Tests.Integration.Observable
{
    public struct QueueOptions
    {
        public static implicit operator QueueOptions(string name)
        {
            return new QueueOptions { Name = name };
        }

        public string Name { get; set; }
    }
}