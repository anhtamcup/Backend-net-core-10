using POS_App.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace POS_App.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        public LoginViewModel(MainViewModel main)
        {
            _main = main;
        }

        public void Login()
        {
            AppServices.Session.CurrentUser = new Dto.UserDto
            {
                ID = 1,
                Code = "001",
                Name = "Lê Anh Tâm"
            };

            //AppServices.Session.AccessToken = token;

            bool success = true;

            if (success)
            {
                _main.OpenOrder();
            }
        }
    }
}
