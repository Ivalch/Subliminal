using System;
using System.Collections.Generic;
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

        private static readonly DataContractJsonSerializer Serializer =
            new DataContractJsonSerializer(typeof(AppSettings));

        [DataMember]
        public bool StartWithWindows { get; set; }

        /// <summary>Overlay colour as #RRGGBB. Stored as hex so it stays readable in JSON.</summary>
        [DataMember]
        public string EditColorHex { get; set; } = "#60A5FA";

        /// <summary>Overlay opacity as a percentage, 0-100.</summary>
        [DataMember]
        public int Transparency { get; set; } = 95;

        /// <summary>
        /// One stored string per line typed in the Settings text editor, so each line is an
        /// independent setting rather than one blob with embedded newlines.
        /// </summary>
        [DataMember]
        public List<string> TextLines { get; set; } = new List<string>();

        /// <summary>
        /// Splits editor text into individual stored strings. Blank lines are dropped so a
        /// trailing newline cannot become a phantom empty entry.
        /// </summary>
        public static List<string> SplitLines(string text)
        {
            var lines = new List<string>();

            if (string.IsNullOrEmpty(text))
            {
                return lines;
            }

            var raw = text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            foreach (var line in raw)
            {
                var trimmed = line.Trim();
                if (trimmed.Length > 0)
                {
                    lines.Add(trimmed);
                }
            }

            return lines;
        }

        /// <summary>Joins stored strings back into editor text, one per line.</summary>
        public static string JoinLines(List<string> lines)
        {
            if (lines == null || lines.Count == 0)
            {
                return string.Empty;
            }

            var kept = new List<string>(lines.Count);
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    kept.Add(line);
                }
            }

            return string.Join(Environment.NewLine, kept);
        }

        public static AppSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                {
                    return new AppSettings();
                }

                AppSettings loaded;
                using (var stream = File.OpenRead(SettingsPath))
                {
                    loaded = Serializer.ReadObject(stream) as AppSettings;
                }

                return loaded == null ? new AppSettings() : Normalize(loaded);
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
            var directory = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (TextLines == null)
            {
                TextLines = new List<string>();
            }

            // File.Create writes a raw stream with no byte-order mark. The serializer
            // throws when it reads a BOM, which would make every load fall back to
            // defaults and silently discard the saved settings.
            using (var stream = File.Create(SettingsPath))
            {
                Serializer.WriteObject(stream, this);
            }

        }

        /// <summary>
        /// Restores invariants the deserializer does not guarantee. DataContractJsonSerializer
        /// creates instances without running field initialisers, so reference-typed members
        /// arrive null even when the file omits them.
        /// </summary>
        private static AppSettings Normalize(AppSettings settings)
        {
            if (settings.TextLines == null)
            {
                settings.TextLines = new List<string>();
            }

            if (settings.EditColorHex == null)
            {
                settings.EditColorHex = "#60A5FA";
            }

            if (settings.Transparency < 0)
            {
                settings.Transparency = 0;
            }
            else if (settings.Transparency > 100)
            {
                settings.Transparency = 100;
            }

            return settings;
        }
    }
}
