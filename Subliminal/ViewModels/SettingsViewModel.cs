using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Subliminal.Converters;
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
        private string _editColorHex;

        [ObservableProperty]
        private int _transparency;

        [ObservableProperty]
        private string _text;

        /// <summary>Copies persisted values into the editable copies the UI binds to.</summary>
        public void Load()
        {
            StartWithWindows = _settings.StartWithWindows;
            EditColorHex = _settings.EditColorHex;
            Transparency = _settings.Transparency;
            Text = _settings.Text;
        }

        /// <summary>Current colour for the colour picker. Falls back to the default if unparseable.</summary>
        public System.Drawing.Color CurrentColor
        {
            get
            {
                System.Drawing.Color color;
                return HexColor.TryParse(EditColorHex, out color) ? color : System.Drawing.Color.CornflowerBlue;
            }
        }

        [RelayCommand]
        private void Save()
        {
            // Transparency is bounded in the model too, not just the slider, so a bad
            // settings file cannot push it out of range.
            _settings.StartWithWindows = StartWithWindows;
            _settings.EditColorHex = EditColorHex;
            _settings.Transparency = Transparency < 0 ? 0 : (Transparency > 100 ? 100 : Transparency);
            _settings.Text = Text ?? string.Empty;
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
