using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using System.Linq;
using Roger.Chat.Messages;

namespace Roger.Chat.Client
{
    [Export]
    public class ShellViewModel : Screen, IConsumer<InstantMessage>, IConsumer<CurrentClients>, IConsumer<CurrentMessages>, IConsumer<ClientConnected>, IConsumer<ClientDisconnected>
    {
        private readonly IRabbitBus bus;
        private readonly IWindowManager windowManager;
        private ObservableCollection<InstantMessageViewModel> instantMessages = new ObservableCollection<InstantMessageViewModel>();
        private ObservableCollection<ClientViewModel> clients;
        private string username;

        [ImportingConstructor]
        public ShellViewModel(IRabbitBus bus, IWindowManager windowManager)
        {
            this.bus = bus;
            this.windowManager = windowManager;
            Clients = new ObservableCollection<ClientViewModel>();
        }

        public ObservableCollection<ClientViewModel> Clients
        {
            get { return clients; }
            set
            {
                clients = value;
                NotifyOfPropertyChange(() => Clients);
            }
        }

        public void Send(string message)
        {
            bus.Publish(new InstantMessage{Username = username, Contents = message});
        }

        public ObservableCollection<InstantMessageViewModel> InstantMessages
        {
            get { return instantMessages; }
            set
            {
                instantMessages = value;
                NotifyOfPropertyChange(() => InstantMessages);
            }
        }

        public void Consume(InstantMessage message)
        {
            Execute.OnUIThread(() => instantMessages.Add(new InstantMessageViewModel{Username = message.Username, Contents = message.Contents}));
        }

        public void Consume(CurrentClients message)
        {
            Execute.OnUIThread(() =>
            {
                if (message.Clients != null)
                    message.Clients.Apply(
                        pair => clients.Add(new ClientViewModel {Username = pair.Username, Endpoint = pair.Endpoint}));
            });
        }

        public void Consume(CurrentMessages message)
        {
            Execute.OnUIThread(() =>
            {
                if (message.Messages != null)
                    message.Messages.Apply(
                        m => instantMessages.Add(new InstantMessageViewModel {Username = m.Username, Contents = m.Contents}));
            });
        }

        public void Consume(ClientConnected message)
        {
            var currentMessage = bus.CurrentMessage;

            Execute.OnUIThread(() => clients.Add(new ClientViewModel{Username = message.Username, Endpoint = currentMessage.Endpoint}));
        }

        public void Consume(ClientDisconnected message)
        {
            Execute.OnUIThread(() => clients.Remove(clients.Single(c => c.Endpoint.Equals(bus.CurrentMessage.Endpoint))));
        }

        protected override void  OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            var promptUsernameViewModel = new PromptUsernameViewModel();

            windowManager.ShowDialog(promptUsernameViewModel);
            username = promptUsernameViewModel.Username;

            bus.Publish(new ClientConnected {Username = username});
        }
    }
}