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

        protected void Run(Action<int> action, int number)
        {
            for (int i = 0; i < number; i++)
            {
                int copy = i;
                Run(() => action(copy));
            }
        }
    }
}