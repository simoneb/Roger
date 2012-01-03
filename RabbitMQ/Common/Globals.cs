namespace Common
{
    public static class Globals
    {
        public const string MainHostName = "localhost";
        public const int MainConnectionPort = 5672;
        public const string MainVirtualHost = "/";
        public const string SecondaryHostName = MainHostName;
        public const int SecondaryConnectionPort = 5674;
        public const string SecondaryVirtualHost = "secondary";

        public const int FederationConnectionPort = 5673;
        public const string FederationExchangeName = "FederationExchange";
    }
}