using System.Diagnostics;
using System.Text;

namespace Tests.Integration.Utils
{
    public class TcpTrace
    {
        private static string _tcpTraceExecutablePath;
        private Process m_process;

        public TcpTrace(string tcpTraceExecutablePath)
        {
            _tcpTraceExecutablePath = tcpTraceExecutablePath;
        }

        public void Start(int listenPort, string serverName, int serverPort, string title)
        {
            var args = new StringBuilder("/listen ")
                .Append(listenPort)
                .Append(" /serverPort ")
                .Append(serverPort)
                .Append(" /serverName ")
                .Append(serverName)
                .Append(" /title \"")
                .Append(title)
                .Append("\"");

            m_process = StartProcess(args.ToString());
        }

        private static Process StartProcess(string arguments)
        {
            return Process.Start(new ProcessStartInfo(_tcpTraceExecutablePath, arguments)
            {
                WindowStyle = ProcessWindowStyle.Minimized
            });
        }

        public void Stop()
        {
            if (m_process != null) 
                m_process.CloseMainWindow();
        }

        public static void StopAll()
        {
            StartProcess("/kill");
        }
    }
}