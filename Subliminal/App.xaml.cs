using System;
using System.Windows;
using Subliminal.Services;
using Subliminal.ViewModels;

namespace Subliminal
{
    public partial class App : Application
    {
        private AppSettings _settings;
        private TrayIconService _trayIcon;
        private SettingsWindow _settingsWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // This app has no main window: it lives in the tray and only ever shows
            // Settings. Without explicit shutdown the process would have nothing keeping
            // it alive once the last window closes.
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            _settings = AppSettings.Load();

            _trayIcon = new TrayIconService();
            _trayIcon.SettingsRequested += OnSettingsRequested;
            _trayIcon.ExitRequested += OnExitRequested;
        }

        private void OnSettingsRequested(object sender, EventArgs e)
        {
            ShowSettings();
        }

        private void ShowSettings()
        {
            if (_settingsWindow == null)
            {
                _settingsWindow = new SettingsWindow
                {
                    DataContext = new SettingsViewModel(_settings)
                };

                _settingsWindow.Closed += OnSettingsWindowClosed;
            }
            else
            {
                // Re-read so reopening shows current values rather than stale edits.
                ((SettingsViewModel)_settingsWindow.DataContext).Load();
            }

            if (_settingsWindow.IsVisible)
            {
                // Already open: raise the existing window instead of creating a second one.
                if (_settingsWindow.WindowState == WindowState.Minimized)
                {
                    _settingsWindow.WindowState = WindowState.Normal;
                }

                _settingsWindow.Activate();
                return;
            }

            // Shown non-modally: there is no owner window, and a modal loop would
            // needlessly block the app while Settings is open.
            _settingsWindow.Show();
            _settingsWindow.Activate();
        }

        private void OnSettingsWindowClosed(object sender, EventArgs e)
        {
            _settingsWindow = null;
        }

        private void OnExitRequested(object sender, EventArgs e)
        {
            _settingsWindow?.Close();
            _trayIcon.Dispose();
            Shutdown();
        }
    }
}
