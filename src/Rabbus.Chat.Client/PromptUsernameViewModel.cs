using Caliburn.Micro;

namespace Rabbus.Chat.Client
{
    public class PromptUsernameViewModel : PropertyChangedBase
    {
        private string username;
        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                NotifyOfPropertyChange(() => Username);
            }
        }
    }
}