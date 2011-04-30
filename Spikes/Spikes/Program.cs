using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Common;
using System.Linq;

namespace Spikes
{
    static class Program
    {
        static readonly BinaryFormatter Formatter = new BinaryFormatter();

        static void Main(string[] args)
        {
            Run<Tutorial6.Runner>(args);
        }

        private static void Run<T>(IEnumerable<string> args) where T : IProcessesProvider
        {
            if(args.Any())
            {
                RunProcess(args);
            }
            else
            {
                SpawnProcesses<T>();
            }
        }

        private static void SpawnProcesses<T>() where T : IProcessesProvider
        {
            var processProvider = Activator.CreateInstance<T>();

            foreach (var process in processProvider.Processes)
                SpawnProcess(process);

            Console.WriteLine("Type the number corresponding to the additional instance of the process and press enter to launch:");
            Console.WriteLine();

            var distinctProcesses = processProvider.Processes.Distinct(new ToStringEqualityComparer<IProcess>()).ToArray();

            for (var i = 0; i < distinctProcesses.Length; i++)
                Console.WriteLine("{0} - {1}", i + 1, distinctProcesses[i].ToString());

            Console.WriteLine();
            Console.WriteLine("Press enter to exit");
            Console.WriteLine();

            while (true)
            {
                int choice;

                var line = Console.ReadLine();

                if (string.Empty.Equals(line))
                    break;

                if (int.TryParse(line, out choice) && choice <= distinctProcesses.Length)
                    SpawnProcess(distinctProcesses[choice-1]);
            }
        }

        private static void SpawnProcess(IProcess process)
        {
            using (var serialized = new MemoryStream())
            {
                Formatter.Serialize(serialized, process);

                Process.Start(Environment.GetCommandLineArgs().First(), Convert.ToBase64String(serialized.GetBuffer()));
            }
        }

        private static void RunProcess(IEnumerable<string> args)
        {
            using (var stream = new MemoryStream(Convert.FromBase64String(args.First())))
            {
                var process = (IProcess) Formatter.Deserialize(stream);

                Console.Title = process.ToString();

                RunProcess(process);
            }
        }

        private static void RunProcess(IProcess process)
        {
            var waitHandle = new ManualResetEvent(false);
            Task.Factory.StartNew(() => process.Start(waitHandle), TaskCreationOptions.LongRunning).ContinueWith(PrintException, TaskContinuationOptions.OnlyOnFaulted);

            Console.WriteLine("Press enter to exit");
            Console.WriteLine();
            Console.ReadLine();

            waitHandle.Set();
        }

        private static void PrintException(Task obj)
        {
            Console.WriteLine(obj.Exception);
        }
    }
}