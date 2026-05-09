using System;
using System.Configuration;
using System.Reflection;
using System.Windows.Forms;
using BeTong.Helpers;

namespace BeTong.Models
{
    public static class CurrentUserContext
    {
        // In-memory current user
        public static ApplicationUserInfo CurrentUser { get; private set; }

        public static bool IsLoggedIn => CurrentUser != null;

        // Gọi khi đăng nhập thành công
        public static void Save(ApplicationUserInfo user)
        {
            CurrentUser = user ?? throw new ArgumentNullException(nameof(user));

            var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            SetAppSetting(config, "CurrentUserId", user.Id);
            SetAppSetting(config, "CurrentUserName", user.UserName);
            SetAppSetting(config, "CurrentGroupId", user.GroupId); // Đảm bảo GroupId được lưu
            SetAppSetting(config, "CurrentCompanyId", user.CompanyId);
            SetAppSetting(config, "CurrentUserCreateBy", user.CreateBy);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        // Xóa trạng thái đăng nhập trong bộ nhớ và xóa thông tin "remember me" đã lưu
        public static void Clear()
        {
            CurrentUser = null;

            try
            {
                // Xóa file/setting "remember me" nếu dùng LoginSettings helper
                LoginSettings.Clear();
            }
            catch
            {
                // Không ném lỗi nếu xóa thất bại — không block luồng logout
            }
        }

        // Thực hiện logout: clear context rồi restart ứng dụng để quay về màn hình đăng nhập khởi động
        // (Việc restart giả định logic khởi động sẽ hiển thị LoginForm)
        public static void Logout()
        {
            Clear();

            // Restart ứng dụng để chạy lại flow khởi động (show LoginForm).
            // Nếu bạn muốn thay đổi hành vi (ví dụ: hiển thị LoginForm modal ngay lập tức),
            // thay Application.Restart() bằng logic mở LoginForm.
            try
            {
                Application.Restart();
            }
            catch
            {
                // Nếu restart không khả dụng, fallback: thoát ứng dụng
                try { Application.Exit(); } catch { /* ignore */ }
            }
        }

        private static void SetAppSetting(Configuration config, string key, string value)
        {
            if (config.AppSettings.Settings[key] == null)
            {
                config.AppSettings.Settings.Add(key, value ?? "");
            }
            else
            {
                config.AppSettings.Settings[key].Value = value ?? "";
            }
        }


        // (ví dụ) thêm handler cho menu hoặc nút Logout trong MainForm
        private static void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var answer = MessageBox.Show("Bạn có muốn đăng xuất?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer == DialogResult.Yes)
                {
                    Logout();
                }
            }
            catch
            {
                // Swallow exceptions from UI handler to avoid crashes on unexpected errors
                try { Logout(); } catch { /* ignore */ }
            }
        }
    }
}
