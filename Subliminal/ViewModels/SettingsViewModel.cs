using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Subliminal.Converters;
using Subliminal.Services;
using System;
using System.Windows;
using System.Windows.Forms;

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
        private int _appearSeconds;

        [ObservableProperty]
        private int _showTimeSeconds;

        [ObservableProperty]
        private string _text;

        /// <summary>Copies persisted values into the editable copies the UI binds to.</summary>
        public void Load()
        {
            StartWithWindows = _settings.StartWithWindows;
            EditColorHex = _settings.EditColorHex;
            Transparency = _settings.Transparency;
            AppearSeconds = _settings.AppearSeconds;
            ShowTimeSeconds = _settings.ShowTimeSeconds;

            // Stored one string per line; the editor shows them as a single block.
            Text = AppSettings.JoinLines(_settings.TextLines);
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
            _settings.AppearSeconds = ClampSeconds(AppearSeconds);
            _settings.ShowTimeSeconds = ClampSeconds(ShowTimeSeconds);

            // Every non-blank line becomes its own stored setting.
            _settings.TextLines = AppSettings.SplitLines(Text);
            try
            {
                _settings.Save();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to save settings: " + ex.Message, "Save error", 
                    System.Windows.MessageBoxButton.OK);
            }

            try
            {
                StartupRegistration.SetEnabled(StartWithWindows);
            }
            catch (System.Exception ex)
            {
                // Registry writes can be denied by policy; the preference is still stored.
                System.Windows.MessageBox.Show("Failed to set startup setting: " + ex.Message, "Save error", 
                    System.Windows.MessageBoxButton.OK);
            }
        }

        /// <summary>Keeps a phase duration inside the 1-10 second range the sliders allow.</summary>
        private static int ClampSeconds(int value)
        {
            if (value < 1)
            {
                return 1;
            }

            return value > 10 ? 10 : value;
        }
    }
}
