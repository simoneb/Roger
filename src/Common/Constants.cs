namespace Common
{
    public static class Constants
    {
        public const string HostName = "localhost";
        public const int MainPort = 5672;
        public const int AlternativeConnectionPort = 5674;
        public const string MainVirtualHost = "/";
        public const string SecondaryVirtualHost = "secondary";

        public const int FederationConnectionPort = 5673;
        public const string FederationExchangeName = "FederationExchange";
    }
}