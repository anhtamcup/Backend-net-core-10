using Notification.Core;
using Notification.Wpf;
using System.Windows;

namespace POS_App.Helpers
{
    public static class ToastService
    {
        private static readonly NotificationManager _manager = new NotificationManager();

        public static void Success(string message, string title = "Thành công")
            => Show(title, message, NotificationType.Success);

        public static void Error(string message, string title = "Lỗi")
            => Show(title, message, NotificationType.Error);

        public static void Warning(string message, string title = "Cảnh báo")
            => Show(title, message, NotificationType.Warning);

        public static void Info(string message, string title = "Thông tin")
            => Show(title, message, NotificationType.Information);

        private static void Show(string title, string message, NotificationType type)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _manager.Show(
                    new NotificationContent
                    {
                        Title = title,
                        Message = message,
                        Type = type
                    },
                    areaName: "GlobalToastArea",
                    expirationTime: TimeSpan.FromSeconds(3)
                );
            });
        }
    }
}
