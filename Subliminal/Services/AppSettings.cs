using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Subliminal.Services
{
    /// <summary>
    /// User preferences, persisted as JSON under %AppData%\Subliminal\settings.json.
    /// </summary>
    [DataContract]
    public class AppSettings
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Subliminal",
            "settings.json");

        [DataMember]
        public bool StartWithWindows { get; set; }

        [DataMember]
        public bool ShowTrayNotifications { get; set; } = true;

        public static AppSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                {
                    return new AppSettings();
                }

                var serializer = new DataContractJsonSerializer(typeof(AppSettings));
                using (var stream = File.OpenRead(SettingsPath))
                {
                    return serializer.ReadObject(stream) as AppSettings ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                // A missing or corrupt settings file must never stop the app from starting.
                System.Diagnostics.Debug.WriteLine("Failed to load settings: " + ex.Message);
                return new AppSettings();
            }
        }

        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var serializer = new DataContractJsonSerializer(typeof(AppSettings));
                using (var stream = File.Create(SettingsPath))
                {
                    serializer.WriteObject(stream, this);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to save settings: " + ex.Message);
            }
        }
    }
}
