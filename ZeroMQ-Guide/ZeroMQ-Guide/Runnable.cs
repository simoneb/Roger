using System;
using System.Threading.Tasks;

namespace ZeroMQ_Guide
{
    public abstract class Runnable : IRunnable
    {
        protected void Run(Action action)
        {
            Task.Factory.StartNew(action, TaskCreationOptions.LongRunning).ContinueWith(DisplayException, TaskContinuationOptions.OnlyOnFaulted);
        }

        private static void DisplayException(Task obj)
        {
            Console.WriteLine(obj.Exception);
        }

        public abstract void Run();
    }
}