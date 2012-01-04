using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Common;
using System.Linq;

namespace Runner
{
    static class Program
    {
        static readonly BinaryFormatter Formatter = new BinaryFormatter();

        static void Main(string[] args)
        {
            Run<Federation.Runner>(args);
        }

        private static void Run<T>(IEnumerable<string> args) where T : IProcessesProvider
        {
            if (args.Any())
                ExecuteProcess(args);
            else
                SpawnChildProcesses<T>();
        }

        private static void SpawnChildProcesses<T>() where T : IProcessesProvider
        {
            var processProvider = Activator.CreateInstance<T>();

            foreach (var process in processProvider.Processes)
                SpawnChildProcess(process);

            Console.WriteLine("Type the number corresponding to the additional instance of the process and press enter to launch:");
            Console.WriteLine();

            var distinctProcesses = processProvider.Processes.Distinct(new ToStringEqualityComparer<IProcess>()).ToArray();

            for (var i = 0; i < distinctProcesses.Length; i++)
                Console.WriteLine("{0} - {1}", i + 1, distinctProcesses[i].ToString());

            while (true)
            {
                int choice;

                var line = Console.ReadLine();

                if (int.TryParse(line, out choice) && choice <= distinctProcesses.Length)
                    SpawnChildProcess(distinctProcesses[choice-1]);
            }
        }

        private static void SpawnChildProcess(IProcess process)
        {
            using (var serialized = new MemoryStream())
            {
                Formatter.Serialize(serialized, process);

                Process.Start(Environment.GetCommandLineArgs().First(), Convert.ToBase64String(serialized.GetBuffer()));
            }
        }

        private static void ExecuteProcess(IEnumerable<string> args)
        {
            using (var stream = new MemoryStream(Convert.FromBase64String(args.First())))
            {
                var process = (IProcess) Formatter.Deserialize(stream);

                Console.Title = process.ToString();

                Execute(process);
            }
        }

        private static void Execute(IProcess process)
        {
            var waitHandle = new ManualResetEvent(false);

            Task.Factory.StartNew(() => process.Start(waitHandle), TaskCreationOptions.LongRunning)
                .ContinueWith(PrintException, TaskContinuationOptions.OnlyOnFaulted);

            Console.CancelKeyPress += delegate
            {
                Console.WriteLine("Exiting...");
                waitHandle.Set();
            };

            waitHandle.WaitOne();
        }

        private static void PrintException(Task obj)
        {
            Console.WriteLine(obj.Exception);
        }
    }
}