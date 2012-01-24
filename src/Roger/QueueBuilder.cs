using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Roger
{
    public class QueueBuilder
    {
        readonly StringBuilder queue = new StringBuilder();

        public QueueBuilder Username
        {
            get 
            { 
                queue.Append(Environment.UserName);
                return this;
            }
        }

        public QueueBuilder MachineName
        {
            get
            {
                queue.Append(Environment.MachineName);
                return this;
            }
        }

        public QueueBuilder Guid
        {
            get
            {
                queue.Append(System.Guid.NewGuid());
                return this;
            }
        }

        public QueueBuilder ProcessId
        {
            get
            {
                queue.Append(Process.GetCurrentProcess().Id);
                return this;
            }
        }

        public QueueBuilder ExecutableName
        {
            get
            {
                queue.Append(Assembly.GetEntryAssembly().GetName().Name);
                return this;
            }
        }

        public QueueBuilder Value(string text)
        {
            queue.Append(text);
            return this;
        }

        public static implicit operator string(QueueBuilder builder)
        {
            return builder.queue.ToString();
        }
    }
}