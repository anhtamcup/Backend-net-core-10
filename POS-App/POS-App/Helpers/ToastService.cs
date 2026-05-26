using POS_App.Views.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace POS_App.Helpers
{
    public static class ToastService
    {
        public static void Show(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var toast = new ToastWindow(message);
                toast.Show();
            });
        }
    }
}
