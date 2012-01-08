using System;
using Roger;

namespace Tests.Integration.Bus.SupportClasses
{
    public class StandardOutLog : IRogerLog
    {
        public void Debug(string message)
        {
            Console.WriteLine(message);
        }

        public void DebugFormat(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }

        public void Info(string message)
        {
            Console.WriteLine(message);
        }

        public void InfoFormat(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }

        public void Warn(string message)
        {
            Console.WriteLine(message);
        }

        public void WarnFormat(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }

        public void Error(string message)
        {
            Console.WriteLine(message);
        }

        public void ErrorFormat(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }

        public void Fatal(string message)
        {
            Console.WriteLine(message);
        }

        public void FatalFormat(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }
    }
}