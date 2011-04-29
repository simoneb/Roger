﻿using System;
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
        static readonly BinaryFormatter formatter = new BinaryFormatter();

        static void Main(string[] args)
        {
            Run<Tutorial5.Runner>(args);
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

            var distinct = processProvider.Processes.Distinct(new ToStringEqualityComparer<IProcess>()).ToArray();

            for (int i = 0; i < distinct.Length; i++)
            {
                Console.WriteLine("{0} - {1}", i+1, distinct[i].ToString());
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
            using (var serialized = new MemoryStream())
            {
                formatter.Serialize(serialized, process);

                Process.Start(Environment.GetCommandLineArgs().First(), Convert.ToBase64String(serialized.GetBuffer()));
            }
        }

        private static void RunProcess(IEnumerable<string> args)
        {
            using (var stream = new MemoryStream(Convert.FromBase64String(args.First())))
            {
                var process = (IProcess) formatter.Deserialize(stream);

                Console.Title = process.ToString();

                RunProcess(process);
            }
        }

        private static void RunProcess(IProcess process)
        {
            var waitHandle = new ManualResetEvent(false);
            Task.Factory.StartNew(() => process.Start(waitHandle), TaskCreationOptions.LongRunning).ContinueWith(PrintException, TaskContinuationOptions.OnlyOnFaulted);

            Console.WriteLine("Press enter to exit ;)");
            Console.ReadLine();

            waitHandle.Set();
        }

        private static void PrintException(Task obj)
        {
            Console.WriteLine(obj.Exception);
        }
    }
}