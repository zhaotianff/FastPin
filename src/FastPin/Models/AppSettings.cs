using System;
using System.IO;
using System.Text.Json;

namespace FastPin.Models
{
    /// <summary>
    /// Application settings model
    /// NOTE: MySQL password is stored in plain text in settings.json.
    /// For production use, consider implementing secure credential storage
    /// such as Windows Credential Manager or encrypted configuration.
    /// </summary>
    public class AppSettings
    {
        public string Language { get; set; } = "en-US";
        public string DatabaseType { get; set; } = "SQLite";
        public string? MySqlServer { get; set; }
        public int? MySqlPort { get; set; }
        public string? MySqlDatabase { get; set; }
        public string? MySqlUsername { get; set; }
        public string? MySqlPassword { get; set; }

        private static string GetSettingsFilePath()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "FastPin"
            );

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            return Path.Combine(appDataPath, "settings.json");
        }

        public static AppSettings Load()
        {
            try
            {
                var filePath = GetSettingsFilePath();
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }

            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                var filePath = GetSettingsFilePath();
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save settings: {ex.Message}", ex);
            }
        }
    }
}
