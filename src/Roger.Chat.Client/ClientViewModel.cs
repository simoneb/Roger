using Caliburn.Micro;

namespace Roger.Chat.Client
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