using Roger;

namespace Tests.Integration.Bus.SupportClasses
{
    public class DebugLog : IRogerLog
    {
        public void Debug(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void DebugFormat(string message, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(message, args);
        }

        public void Info(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void InfoFormat(string message, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(message, args);
        }

        public void Warn(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void WarnFormat(string message, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(message, args);
        }

        public void Error(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void ErrorFormat(string message, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(message, args);
        }

        public void Fatal(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void FatalFormat(string message, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(message, args);
        }
    }
}