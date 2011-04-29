using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using System.Linq;

namespace Spikes
{
    class Program
    {
        static void Main(string[] args)
        {
            Run<Tutorial3.Runner>(args);
        }

        private static void Run<T>(IEnumerable<string> args) where T : IProcessesProvider
        {
            if(args.Any())
            {
                RunProcess(args);
            }
            else
            {
                StartExternalProcesses<T>();
            }
        }

        private static void StartExternalProcesses<T>() where T : IProcessesProvider
        {
            var processProvider = Activator.CreateInstance<T>();

            foreach (var process in processProvider.Processes)
                StartExternalProcess(process);

            var distinct = processProvider.Processes.Distinct(new ByTypeEqualityComparer<IProcess>()).ToArray();

            for (int i = 0; i < distinct.Length; i++)
            {
                Console.WriteLine("{0} - {1}", i+1, distinct[i].GetType().Name);
            }

            Console.WriteLine();

            while (true)
            {
                int choice;

                if (int.TryParse(Console.ReadLine(), out choice) && choice <= distinct.Length)
                    StartExternalProcess(distinct[choice-1]);
            }
        }

        private static void StartExternalProcess(IProcess process)
        {
            Process.Start(Environment.GetCommandLineArgs().First(),
                          Convert.ToBase64String(Encoding.UTF8.GetBytes(process.GetType().AssemblyQualifiedName)));
        }

        private static void RunProcess(IEnumerable<string> args)
        {
            var type = Type.GetType(Encoding.UTF8.GetString(Convert.FromBase64String(args.First())));

            Console.Title = type.ToString();

            var process = (IProcess) Activator.CreateInstance(type);

            RunProcess(process);
        }

        private static void RunProcess(IProcess process)
        {
            var waitHandle = new ManualResetEvent(false);
            Task.Factory.StartNew(() => process.Start(waitHandle), TaskCreationOptions.LongRunning);

            Console.WriteLine("Press enter to exit ;)");
            Console.ReadLine();

            waitHandle.Set();
        }
    }
}