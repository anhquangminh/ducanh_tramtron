using System;
using System.IO;

namespace BeTong.Helpers
{
    public class UiSettings
    {
        public const float DefaultFontSize = 12F;
        public const int DefaultStartupWidthPercent = 100;

        public float FontSize { get; set; }
        public int StartupWidthPercent { get; set; }

        private static string SettingsDirectory
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BeTong"); }
        }

        private static string SettingsFilePath
        {
            get { return Path.Combine(SettingsDirectory, "ui.settings"); }
        }

        public static UiSettings Load()
        {
            var settings = new UiSettings
            {
                FontSize = DefaultFontSize,
                StartupWidthPercent = DefaultStartupWidthPercent
            };

            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    return settings;
                }

                var lines = File.ReadAllLines(SettingsFilePath);
                foreach (var line in lines)
                {
                    if (line == null || line.Trim().Length == 0) continue;
                    var idx = line.IndexOf('=');
                    if (idx <= 0) continue;

                    var key = line.Substring(0, idx).Trim();
                    var value = line.Substring(idx + 1).Trim();

                    switch (key)
                    {
                        case "FontSize":
                            float fontSize;
                            if (float.TryParse(value, out fontSize))
                            {
                                settings.FontSize = ClampFontSize(fontSize);
                            }
                            break;
                        case "StartupWidthPercent":
                            int widthPercent;
                            if (int.TryParse(value, out widthPercent))
                            {
                                settings.StartupWidthPercent = ClampWidthPercent(widthPercent);
                            }
                            break;
                    }
                }
            }
            catch
            {
                return new UiSettings
                {
                    FontSize = DefaultFontSize,
                    StartupWidthPercent = DefaultStartupWidthPercent
                };
            }

            settings.FontSize = ClampFontSize(settings.FontSize);
            settings.StartupWidthPercent = ClampWidthPercent(settings.StartupWidthPercent);
            return settings;
        }

        public static void Save(UiSettings settings)
        {
            try
            {
                if (!Directory.Exists(SettingsDirectory))
                {
                    Directory.CreateDirectory(SettingsDirectory);
                }

                var fontSize = ClampFontSize(settings == null ? DefaultFontSize : settings.FontSize);
                var widthPercent = ClampWidthPercent(settings == null ? DefaultStartupWidthPercent : settings.StartupWidthPercent);
                var lines = new[]
                {
                    "FontSize=" + fontSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    "StartupWidthPercent=" + widthPercent.ToString(System.Globalization.CultureInfo.InvariantCulture)
                };

                File.WriteAllLines(SettingsFilePath, lines);
            }
            catch
            {
                // UI settings must never block application startup.
            }
        }

        public static float ClampFontSize(float value)
        {
            if (value < 8F) return 8F;
            if (value > 24F) return 24F;
            return value;
        }

        public static int ClampWidthPercent(int value)
        {
            if (value < 10) return 10;
            if (value > 100) return 100;
            return value;
        }
    }
}
