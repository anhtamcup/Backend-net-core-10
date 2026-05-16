using System;
using System.Collections.Generic;
using System.Text;
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
            POSNumber = "POS-01";
            Code = "HD000123";
            StaffCode = "NV001";
            ShiftID = "SHIFT-A";
            StaffName = "Lê Anh Tâm";

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            timer.Tick += (_, _) =>
            {
                CurrentTime =
                    DateTime.Now.ToString(
                        "dd-MM-yyyy HH:mm:ss");
            };

            timer.Start();
        }
    }

    public class MainViewModel: BaseViewModel
    {
        public HeaderViewModel Header { get; } = new();
        public OrderViewModel OrderVM { get; } = new();

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
            CurrentView = OrderVM;
        }
    }
}
