using Caliburn.Micro;

namespace Roger.Chat.Client
{
    public class InstantMessageViewModel : PropertyChangedBase
    {
        private string contents;
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

        public string Contents
        {
            get { return contents; }
            set
            {
                contents = value;
                NotifyOfPropertyChange(() => Contents);
            }
        }
    }
}