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
            Run(repetitions, (i, @null) => action(i), Null.Value);
        }

        protected static void Run<T>(int repetitions, Action<int, T> action, T argument)
        {
            for (var i = 0; i < repetitions; i++)
            {
                var copy = i;
                Run(() => action(copy, argument));
            }
        }

        protected static void Run<T>(Action<T> action, T argument)
        {
            Run(() => action(argument));
        }

        public class Null
        {
            public static readonly Null Value = new Null();
        }
    }
}