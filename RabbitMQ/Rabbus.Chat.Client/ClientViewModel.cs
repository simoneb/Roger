using Caliburn.Micro;

namespace Rabbus.Chat.Client
{
    public class ClientViewModel : PropertyChangedBase
    {
        private string username;

        public string Username
        {
            get {
                return username;
            }
            set {
                username = value;
                NotifyOfPropertyChange(() => Username);
            }
        }

        public string Endpoint { get; set; }
    }
}