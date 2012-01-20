using System.ServiceModel;
using System.Threading;

namespace Tests.Integration.Exploratory.WCF
{
    [ServiceBehavior]
    public class Logger : ILogger
    {
        public static string LastLogged;
        public static readonly EventWaitHandle Semaphore = new AutoResetEvent(false);

        public void Log(string message)
        {
            LastLogged = message;
            Semaphore.Set();
        }
    }
}