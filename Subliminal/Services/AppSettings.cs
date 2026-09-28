using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

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

        /// <summary>Overlay colour as #RRGGBB. Stored as hex so it stays readable in JSON.</summary>
        [DataMember]
        public string EditColorHex { get; set; } = "#60A5FA";

        /// <summary>Overlay opacity as a percentage, 0-100.</summary>
        [DataMember]
        public int Transparency { get; set; } = 95;

        /// <summary>
        /// One stored string per line typed in the Settings text editor. Persisted as a
        /// JSON array so every line is an independent entry rather than one blob with
        /// embedded newlines.
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

                // Reading stays with the framework serializer so malformed or unexpected
                // files are handled properly rather than by hand-rolled parsing.
                var serializer = new DataContractJsonSerializer(typeof(AppSettings));
                AppSettings loaded;
                using (var stream = File.OpenRead(SettingsPath))
                {
                    loaded = serializer.ReadObject(stream) as AppSettings;
                }

                if (loaded == null)
                {
                    return new AppSettings();
                }

                loaded.Normalize();
                return loaded;
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

                if (TextLines == null)
                {
                    TextLines = new List<string>();
                }

                // UTF8Encoding(false) omits the byte-order mark. DataContractJsonSerializer
                // throws when it meets a BOM, which would make every read fall back to
                // defaults and silently discard the saved settings.
                using (var writer = new StreamWriter(SettingsPath, false, new UTF8Encoding(false)))
                {
                    WriteTo(writer);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to save settings: " + ex.Message);
            }
        }

        /// <summary>
        /// Writes the file with each stored string on its own line. The framework's
        /// serializer cannot do this: DataContractJsonSerializerSettings.Indent does not
        /// exist on .NET Framework, so it would emit everything on a single line.
        /// </summary>
        private void WriteTo(TextWriter writer)
        {
            writer.WriteLine("{");
            writer.WriteLine("  \"StartWithWindows\": " + (StartWithWindows ? "true" : "false") + ",");
            writer.WriteLine("  \"EditColorHex\": " + Quote(EditColorHex) + ",");
            writer.WriteLine("  \"Transparency\": " + Transparency.ToString(CultureInfo.InvariantCulture) + ",");
            writer.WriteLine("  \"TextLines\": [");

            for (var i = 0; i < TextLines.Count; i++)
            {
                var comma = i < TextLines.Count - 1 ? "," : string.Empty;
                writer.WriteLine("    " + Quote(TextLines[i]) + comma);
            }

            writer.WriteLine("  ]");
            writer.WriteLine("}");
        }

        private static string Quote(string value)
        {
            if (value == null)
            {
                return "null";
            }

            var sb = new StringBuilder(value.Length + 2);
            sb.Append('"');

            foreach (var c in value)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ')
                        {
                            sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            sb.Append(c);
                        }

                        break;
                }
            }

            sb.Append('"');
            return sb.ToString();
        }

        /// <summary>
        /// Restores invariants the deserializer does not guarantee. DataContractJsonSerializer
        /// creates instances without running field initialisers, so reference-typed members
        /// arrive null even when the file omits them.
        /// </summary>
        private void Normalize()
        {
            if (TextLines == null)
            {
                TextLines = new List<string>();
            }

            if (EditColorHex == null)
            {
                EditColorHex = "#60A5FA";
            }

            if (Transparency < 0)
            {
                Transparency = 0;
            }
            else if (Transparency > 100)
            {
                Transparency = 100;
            }
        }
    }
}
