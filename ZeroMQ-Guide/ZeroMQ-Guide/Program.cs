using System;
using ZeroMQ_Guide.Guide;

namespace ZeroMQ_Guide
{
    class Program
    {
        static void Main()
        {
            Run<NodeCoordination>();
            Console.ReadLine();
        }

        private static void Run<T>() where T : IRunnable
        {
            Activator.CreateInstance<T>().Run();
        }
    }
}