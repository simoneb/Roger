using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Shoveling.Test
{
    public class RabbitMQBroker : IDisposable
    {
        private readonly string m_path;
        private const string ServerExecutableName = "rabbitmq-server.bat";
        private const string ControlExecutableName = "rabbitmqctl.bat";
        private string ServerExecutablePath { get { return Path.Combine(m_path, @"sbin\" + ServerExecutableName); } }
        private string ControlExecutablePath { get { return Path.Combine(m_path, @"sbin\" + ControlExecutableName); } }

        public RabbitMQBroker(string path)
        {
            m_path = path;
        }

        public void Start()
        {
            if (Process.GetProcessesByName(ServerExecutableName)
                .Any(p => p.StartInfo.FileName.Equals(ServerExecutablePath, StringComparison.OrdinalIgnoreCase)))
                return;

            Process.Start(ServerExecutablePath);
            Thread.Sleep(2000);

            Wait();
        }

        private void Wait()
        {
            ControlCommand("wait");
        }

        public void Dispose()
        {
            Stop();
        }

        public void Stop()
        {
            ControlCommand("stop");
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
    }
}