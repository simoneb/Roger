using System;
using System.Threading.Tasks;

namespace ZeroMQ_Guide
{
    public abstract class Runnable : IRunnable
    {
        protected static void Run(Action action)
        {
            Task.Factory.StartNew(action, TaskCreationOptions.LongRunning).ContinueWith(DisplayException, TaskContinuationOptions.OnlyOnFaulted);
        }

        private static void DisplayException(Task obj)
        {
            Console.WriteLine(obj.Exception);
        }

        public abstract void Run();

        protected static void Run(int repetitions, Action<int> action)
        {
            for (int i = 0; i < repetitions; i++)
            {
                int copy = i;
                Run(() => action(copy));
            }
        }

        protected static void Run<T>(Action<T> action, T argument)
        {
            Run(() => action(argument));
        }
    }
}