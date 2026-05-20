using POS_App.Services;
using System.ComponentModel;
using System.Windows.Threading;

namespace POS_App.ViewModels
{
    public class HeaderViewModel : BaseViewModel
    {
        private string _currentTime = "";
        public string CurrentTime
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }

        private string _posNumber = "";
        public string POSNumber
        {
            get => _posNumber;
            set => SetProperty(ref _posNumber, value);
        }

        private string _code = "";
        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        }

        private string _staffCode = "";
        public string StaffCode
        {
            get => _staffCode;
            set => SetProperty(ref _staffCode, value);
        }

        private string _staffName = "";
        public string StaffName
        {
            get => _staffName;
            set => SetProperty(ref _staffName, value);
        }

        private string _shiftID = "";
        public string ShiftID
        {
            get => _shiftID;
            set => SetProperty(ref _shiftID, value);
        }

        public HeaderViewModel()
        {
            SessionService.Instance.PropertyChanged += Session_PropertyChanged;

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            timer.Tick += (_, _) =>
            {
                //CurrentTime = DateTime.Now.ToString("HH:mm:ss dd-MM-yyyy");
                CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            };

            timer.Start();
        }

        private void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SessionService.Instance.CurrentUser))
            {
                LoadUser();
            }
        }

        private void LoadUser()
        {
            var user = SessionService.Instance.CurrentUser;
            StaffCode = user?.Code ?? string.Empty;
            StaffName = user?.Name ?? string.Empty;

            if (user == null)
            {
                POSNumber = string.Empty;
                ShiftID = string.Empty;
            }
            else
            {
                POSNumber = "P01";
                ShiftID = "K001";
            }
        }
    }

    public class MainViewModel : BaseViewModel
    {
        public HeaderViewModel Header { get; } = new();

        private object _currentView;

        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            CurrentView = new LoginViewModel(this);
        }

        public void OpenOrder()
        {
            CurrentView = new OrderViewModel();
        }

        public void Logout()
        {
            SessionService.Instance.Logout();
            CurrentView = new LoginViewModel(this);
        }
    }
}
