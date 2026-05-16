namespace POS_App.ViewModels
{
    public class CustomerInfoViewModel : BaseViewModel
    {
        public CustomerInfoViewModel()
        {
            Name = "Nguyễn Thượng Đế";
            Point = 25000;
        }

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

        private int _point;
        public int Point
        {
            get => _point;
            set => SetProperty(ref _point, value);
        }
    }
}
