using System;
using System.IO;

namespace BeTong.Helpers
{
    public class LoginSettings
    {
        public string LoginName { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }

        private static string SettingsDirectory =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BeTong");

        private static string SettingsFilePath => Path.Combine(SettingsDirectory, "login.settings");

        public static LoginSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    return new LoginSettings { LoginName = string.Empty, Password = string.Empty, RememberMe = false };
                }

                var lines = File.ReadAllLines(SettingsFilePath);
                var s = new LoginSettings();
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var idx = line.IndexOf('=');
                    if (idx <= 0) continue;
                    var key = line.Substring(0, idx).Trim();
                    var value = line.Substring(idx + 1).Trim();
                    switch (key)
                    {
                        case "LoginName":
                            s.LoginName = Uri.UnescapeDataString(value);
                            break;
                        case "Password":
                            s.Password = Uri.UnescapeDataString(value);
                            break;
                        case "RememberMe":
                            bool.TryParse(value, out var rm);
                            s.RememberMe = rm;
                            break;
                    }
                }
                return s;
            }
            catch
            {
                return new LoginSettings { LoginName = string.Empty, Password = string.Empty, RememberMe = false };
            }
        }

        public static void Save(LoginSettings settings)
        {
            try
            {
                if (!Directory.Exists(SettingsDirectory))
                {
                    Directory.CreateDirectory(SettingsDirectory);
                }

                var lines = new[]
                {
                    $"LoginName={Uri.EscapeDataString(settings.LoginName ?? string.Empty)}",
                    $"Password={Uri.EscapeDataString(settings.Password ?? string.Empty)}",
                    $"RememberMe={settings.RememberMe}"
                };
                File.WriteAllLines(SettingsFilePath, lines);
            }
            catch
            {
                // Fail silently — do not block login if settings cannot be saved.
            }
        }

        public static void Clear()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                    File.Delete(SettingsFilePath);
            }
            catch
            {
                // ignore
            }
        }
    }
}