using POS_App.Dto;
using POS_App.ViewModels;

namespace POS_App.Services
{
    public class SessionService : BaseViewModel
    {
        private UserDto _currentUser;
        public UserDto CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
            }
        }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string SessionId { get; set; }

        public bool IsLoggedIn => CurrentUser != null;

        public void Logout()
        {
            CurrentUser = null;
            AccessToken = null;
            RefreshToken = null;
            SessionId = null;
        }
    }
}
