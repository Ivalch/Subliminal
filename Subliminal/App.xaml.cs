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
        private OverlayWindow _overlayWindow;
        private OverlayViewModel _overlayViewModel;

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

            StartOverlay();
        }

        /// <summary>
        /// Creates and shows the text overlay. It runs for the lifetime of the app with no
        /// user-facing toggle; it is torn down by Exit. An empty text list simply renders
        /// nothing, and the first tick after texts are saved starts showing them.
        /// </summary>
        private void StartOverlay()
        {
            _overlayViewModel = new OverlayViewModel(_settings);
            _overlayWindow = new OverlayWindow { DataContext = _overlayViewModel };

            _overlayViewModel.Start();
            _overlayWindow.Show();
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

            // Always refresh from the model. A newly created ViewModel starts empty, so
            // this has to run on every open, not only when reusing an existing window.
            ((SettingsViewModel)_settingsWindow.DataContext).Load();

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

            // Apply anything just saved to the overlay straight away instead of waiting
            // for its next tick.
            _overlayViewModel?.Refresh();
        }

        private void OnExitRequested(object sender, EventArgs e)
        {
            _settingsWindow?.Close();
            _overlayWindow?.Close();
            _trayIcon.Dispose();
            Shutdown();
        }
    }
}
