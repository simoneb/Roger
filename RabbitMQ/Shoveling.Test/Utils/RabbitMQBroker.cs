using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Shoveling.Test.Utils
{
    public class RabbitMQBroker
    {
        private readonly string brokerFolder;
        private Process serverProcess;
        private const string ServerExecutableName = "rabbitmq-server.bat";
        private const string ControlExecutableName = "rabbitmqctl.bat";
        private string ServerExecutablePath { get { return Path.Combine(brokerFolder, @"sbin\" + ServerExecutableName); } }
        private string ControlExecutablePath { get { return Path.Combine(brokerFolder, @"sbin\" + ControlExecutableName); } }

        public RabbitMQBroker(string brokerFolder)
        {
            this.brokerFolder = brokerFolder;
        }

        public void StartAndWait()
        {
            if (Running)
                return;

            serverProcess = Process.Start(ServerExecutablePath);

            Thread.Sleep(2000);
            Wait();
        }

        private bool Running
        {
            get
            {
                return Process.GetProcessesByName(ServerExecutableName)
                    .Any(p => p.StartInfo.FileName.Equals(ServerExecutablePath, StringComparison.OrdinalIgnoreCase));
            }
        }

        private void Wait()
        {
            ControlCommand("wait");
        }

        public void Stop()
        {
            ControlCommand("stop");

            if (serverProcess != null)
                serverProcess.CloseMainWindow();
        }

        private void ControlCommand(string command)
        {
            using (var process = Process.Start(ControlExecutablePath, command))
                process.WaitForExit();
        }

        public void AddVHost(string path)
        {
            ControlCommand("add_vhost", path);
        }

        private void ControlCommand(string command, string argument)
        {
            ControlCommand(command + " " + argument);
        }

        public void DeleteVHost(string path)
        {
            ControlCommand("delete_vhost", path);
        }

        public void AddPermissions(string path, string user)
        {
            ControlCommand("set_permissions", "-p " + path + " " + user + " \".*\" \".*\" \".*\"");
        }

        public void StopApp()
        {
            ControlCommand("stop_app");
        }

        public void Reset()
        {
            ControlCommand("reset");
        }

        public void StartAppAndWait()
        {
            ControlCommand("start_app");
            Wait();
        }
    }
}