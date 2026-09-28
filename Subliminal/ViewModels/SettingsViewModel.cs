using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Subliminal.Services;

namespace Subliminal.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly AppSettings _settings;

        public SettingsViewModel(AppSettings settings)
        {
            _settings = settings;
        }

        [ObservableProperty]
        private bool _startWithWindows;

        [ObservableProperty]
        private bool _showTrayNotifications;

        /// <summary>Copies persisted values into the editable copies the UI binds to.</summary>
        public void Load()
        {
            StartWithWindows = _settings.StartWithWindows;
            ShowTrayNotifications = _settings.ShowTrayNotifications;
        }

        [RelayCommand]
        private void Save()
        {
            _settings.StartWithWindows = StartWithWindows;
            _settings.ShowTrayNotifications = ShowTrayNotifications;
            _settings.Save();

            try
            {
                StartupRegistration.SetEnabled(StartWithWindows);
            }
            catch (System.Exception ex)
            {
                // Registry writes can be denied by policy; the preference is still stored.
                System.Diagnostics.Debug.WriteLine("Failed to update startup registration: " + ex.Message);
            }
        }
    }
}
