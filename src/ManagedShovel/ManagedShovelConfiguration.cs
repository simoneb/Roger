using System;
using RabbitMQ.Client;

namespace ManagedShovel
{
    internal class ManagedShovelConfiguration
    {
        public ManagedShovelConfiguration()
        {
            Sources = new string[0];
            SourceDeclarations = new Action<IModel>[0];
            Destinations = new string[0];
            DestinationDeclarations = new Action<IModel>[0];
            AckMode = AckMode.OnConfirm;
            PublishProperties = new Action<IBasicProperties>[0];
            PublishFields = Tuple.Create<string, string>(null, null);
            ReconnectDelay = TimeSpan.FromSeconds(5);
        }

        public string[] Sources { get; set; }
        public Action<IModel>[] SourceDeclarations { get; set; }
        public string[] Destinations { get; set; }
        public Action<IModel>[] DestinationDeclarations { get; set; }

        public bool LastCreatedQueue { get; set; }
        public string Queue { get; set; }
        public ushort PrefetchCount { get; set; }
        public AckMode AckMode { get; set; }
        public Action<IBasicProperties>[] PublishProperties { get; set; }
        public Tuple<string, string> PublishFields { get; set; }
        public TimeSpan ReconnectDelay { get; set; }
        public int MaxHops { get; set; }
    }
}