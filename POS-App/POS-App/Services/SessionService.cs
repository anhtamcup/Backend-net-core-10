using POS_App.Dto;
using POS_App.Models;
using POS_App.ViewModels;

namespace POS_App.Services
{
    public class SessionService : BaseViewModel
    {
        // ── Singleton ────────────────────────────────────────
        public static SessionService Instance { get; } = new();
        private SessionService() { }

        // ── In-memory state (Observable cho binding) ─────────
        private UserDto _currentUser;
        public UserDto CurrentUser
        {
            get => _currentUser;
            private set { _currentUser = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsLoggedIn)); }
        }

        private AuthSession _session;
        public AuthSession Session
        {
            get => _session;
            private set { _session = value; OnPropertyChanged(); }
        }

        // Shortcuts tiện dùng (không đổi tên để tránh sửa code cũ)
        public string AccessToken => _session?.AccessToken;
        public string RefreshToken => _session?.RefreshToken;
        public string SessionId => _session?.SessionId;

        public bool IsLoggedIn => CurrentUser != null && !TokenStorageService.IsExpired();

        // ── Public API ───────────────────────────────────────

        /// Gọi sau khi server trả về login response
        public void Login(UserDto user, AuthSession session)
        {
            CurrentUser = user;
            Session = session;

            UserStorageService.Save(user);
            TokenStorageService.Save(session);
        }

        /// Gọi khi app khởi động — true nếu còn session hợp lệ
        public bool TryRestore()
        {
            var session = TokenStorageService.Load();
            var user = UserStorageService.Load();

            if (session == null || user == null || TokenStorageService.IsExpired())
            {
                Logout(); // Dọn dẹp nếu dữ liệu không đồng bộ
                return false;
            }

            Session = session;
            CurrentUser = user;
            return true;
        }

        /// Gọi khi logout
        public void Logout()
        {
            CurrentUser = null;
            Session = null;

            UserStorageService.Clear();
            TokenStorageService.Clear();
        }
    }
}
