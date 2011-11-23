using System;
using System.Collections.Concurrent;
using Rabbus.Chat.Messages;
using System.Linq;

namespace Rabbus.Chat.Server
{
    internal class ChatServer : IConsumer<ClientConnected>, IConsumer<ClientDisconnected>, IConsumer<InstantMessage>
    {
        private readonly IRabbitBus bus;
        readonly ConcurrentDictionary<string, string> clients = new ConcurrentDictionary<string, string>();
        readonly BlockingCollection<InstantMessage> messages = new BlockingCollection<InstantMessage>(20);

        public ChatServer(IRabbitBus bus)
        {
            this.bus = bus;
        }

        public void Consume(ClientConnected message)
        {
            Console.WriteLine("New client connected");

            bus.Send(bus.CurrentMessage.Endpoint, new CurrentClients { Clients = clients.Select(p => new Client{Username = p.Value, Endpoint = p.Key}).ToArray() });
            bus.Send(bus.CurrentMessage.Endpoint, new CurrentMessages { Messages = messages.ToArray() });

            clients.TryAdd(bus.CurrentMessage.Endpoint, message.Username);
        }

        public void Consume(ClientDisconnected message)
        {
            Console.WriteLine("Client disconnected");

            string _;
            clients.TryRemove(bus.CurrentMessage.Endpoint, out _);
        }

        public void Consume(InstantMessage message)
        {
            Console.WriteLine("Message received");

            messages.Add(message);
        }
    }
}