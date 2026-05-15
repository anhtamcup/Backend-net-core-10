using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace POS_App.Dto
{
    public class HeaderView : INotifyPropertyChanged
    {
        private string _currentTime;

        public string CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                OnPropertyChanged();
            }
        }

        public HeaderView()
        {
            CurrentTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);

            timer.Tick += (s, e) =>
            {
                CurrentTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
            };

            timer.Start();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(
            [CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(name));
        }
    }
}
