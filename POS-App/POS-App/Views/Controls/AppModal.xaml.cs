using System.Windows;

namespace POS_App.Views.Controls
{
    /// <summary>
    /// Interaction logic for AppModal.xaml
    /// </summary>
    public partial class AppModal : Window
    {
        public AppModal()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            if (Owner != null)
            {
                // Lấy bounds thực tế của owner (kể cả khi maximize)
                var ownerLeft = Owner.WindowState == WindowState.Maximized ? 0 : Owner.Left;
                var ownerTop = Owner.WindowState == WindowState.Maximized ? 0 : Owner.Top;
                var ownerWidth = Owner.WindowState == WindowState.Maximized
                                    ? SystemParameters.WorkArea.Width
                                    : Owner.ActualWidth;
                var ownerHeight = Owner.WindowState == WindowState.Maximized
                                    ? SystemParameters.WorkArea.Height
                                    : Owner.ActualHeight;

                Width = ownerWidth;
                Height = ownerHeight;
                Left = ownerLeft;
                Top = ownerTop;
            }
        }

        public object ModalContent
        {
            get => GetValue(ModalContentProperty);
            set => SetValue(ModalContentProperty, value);
        }

        public static readonly DependencyProperty ModalContentProperty =
            DependencyProperty.Register(
                nameof(ModalContent),
                typeof(object),
                typeof(AppModal));

        public string ModalTitle
        {
            get => (string)GetValue(ModalTitleProperty);
            set => SetValue(ModalTitleProperty, value);
        }

        public static readonly DependencyProperty ModalTitleProperty =
            DependencyProperty.Register(
                nameof(ModalTitle),
                typeof(string),
                typeof(AppModal),
                new PropertyMetadata("Modal"));

        public double ModalWidth
        {
            get => (double)GetValue(ModalWidthProperty);
            set => SetValue(ModalWidthProperty, value);
        }

        public static readonly DependencyProperty ModalWidthProperty =
            DependencyProperty.Register(
                nameof(ModalWidth),
                typeof(double),
                typeof(AppModal),
                new PropertyMetadata(900d));

        public double ModalHeight
        {
            get => (double)GetValue(ModalHeightProperty);
            set => SetValue(ModalHeightProperty, value);
        }

        public static readonly DependencyProperty ModalHeightProperty =
            DependencyProperty.Register(
                nameof(ModalHeight),
                typeof(double),
                typeof(AppModal),
                new PropertyMetadata(560d));

        private void btnCloseModal_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
