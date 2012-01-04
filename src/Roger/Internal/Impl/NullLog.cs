namespace Rabbus.Internal.Impl
{
    internal class NullLog : IRabbusLog
    {
        public void Debug(string message)
        {
        }

        public void DebugFormat(string message, params object[] args)
        {
        }

        public void Info(string message)
        {
        }

        public void InfoFormat(string message, params object[] args)
        {
        }

        public void Warn(string message)
        {
        }

        public void WarnFormat(string message, params object[] args)
        {
        }

        public void Error(string message)
        {
        }

        public void ErrorFormat(string message, params object[] args)
        {
        }

        public void Fatal(string message)
        {
        }

        public void FatalFormat(string message, params object[] args)
        {
        }
    }
}