using System;
using System.Collections.Generic;
using System.Text;

namespace POS_App.ViewModels
{
    internal class MainViewModel: BaseViewModel
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
