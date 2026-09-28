using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace Subliminal.Services
{
    /// <summary>
    /// Manages the per-user "Run at startup" entry via the Windows registry.
    /// </summary>
    internal static class StartupRegistration
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "Subliminal";

        public static void SetEnabled(bool enabled)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true))
            {
                if (key == null)
                {
                    throw new InvalidOperationException("The Windows Run key could not be opened.");
                }

                if (enabled)
                {
                    key.SetValue(ValueName, ExecutablePath(), RegistryValueKind.String);
                }
                else
                {
                    key.DeleteValue(ValueName, throwOnMissingValue: false);
                }
            }
        }

        private static string ExecutablePath()
        {
            // A Run value is parsed as a command line, so paths containing spaces must be quoted.
            // Note: WPF's Application has no ExecutablePath (that is the WinForms one), so the
            // real exe path is read from the running process.
            var path = Process.GetCurrentProcess().MainModule.FileName;
            return path.IndexOf(' ') >= 0 ? "\"" + path + "\"" : path;
        }
    }
}
