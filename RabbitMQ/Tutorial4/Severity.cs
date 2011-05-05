using System;

namespace Tutorial4
{
    static class Severity
    {
        private static readonly string[] All = new[] { Debug, Info, Warning, Error };
        private static readonly Random Rnd = new Random();
        public const string Debug = "debug";
        public const string Info = "info";
        public const string Warning = "warning";
        public const string Error = "error";

        public static string Random()
        {
            return All[Rnd.Next(0, All.Length)];
        }
    }
}