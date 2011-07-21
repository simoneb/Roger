namespace Tests.Integration.Observable
{
    public struct ExchangeOptions
    {
        public static implicit operator ExchangeOptions(string name)
        {
            return new ExchangeOptions { Name = name };
        }

        public string Name { get; set; }
    }
}