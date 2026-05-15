using System;
using System.Collections.Generic;
using System.Text;

namespace POS_App.ViewModels
{
    public class CustomerInfoViewModel : BaseViewModel
    {
        private string _name = "";

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _phone = "";

        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }
    }
}
