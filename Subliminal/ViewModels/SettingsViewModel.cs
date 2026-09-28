using System;
using Subliminal.Converters;
using Subliminal.Mvvm;
using Subliminal.Services;

namespace Subliminal.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly AppSettings _settings;

        private bool _startWithWindows;
        private string _editColorHex;
        private int _transparency;
        private int _appearSeconds;
        private int _showTimeSeconds;
        private int _fontSize;
        private string _text;

        public SettingsViewModel(AppSettings settings)
        {
            _settings = settings;
            SaveCommand = new RelayCommand(Save);
        }

        public bool StartWithWindows
        {
            get { return _startWithWindows; }
            set { SetProperty(ref _startWithWindows, value); }
        }

        public string EditColorHex
        {
            get { return _editColorHex; }
            set { SetProperty(ref _editColorHex, value); }
        }

        public int Transparency
        {
            get { return _transparency; }
            set { SetProperty(ref _transparency, value); }
        }

        public int AppearSeconds
        {
            get { return _appearSeconds; }
            set { SetProperty(ref _appearSeconds, value); }
        }

        public int ShowTimeSeconds
        {
            get { return _showTimeSeconds; }
            set { SetProperty(ref _showTimeSeconds, value); }
        }

        public int FontSize
        {
            get { return _fontSize; }
            set { SetProperty(ref _fontSize, value); }
        }

        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }

        /// <summary>Invoked by the Settings window when the dialog closes.</summary>
        public RelayCommand SaveCommand { get; private set; }

        /// <summary>Copies persisted values into the editable copies the UI binds to.</summary>
        public void Load()
        {
            StartWithWindows = _settings.StartWithWindows;
            EditColorHex = _settings.EditColorHex;
            Transparency = _settings.Transparency;
            AppearSeconds = _settings.AppearSeconds;
            ShowTimeSeconds = _settings.ShowTimeSeconds;
            FontSize = _settings.FontSize;

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

        private void Save()
        {
            // Transparency is bounded in the model too, not just the slider, so a bad
            // settings file cannot push it out of range.
            _settings.StartWithWindows = StartWithWindows;
            _settings.EditColorHex = EditColorHex;
            _settings.Transparency = Transparency < 0 ? 0 : (Transparency > 100 ? 100 : Transparency);
            _settings.AppearSeconds = ClampSeconds(AppearSeconds);
            _settings.ShowTimeSeconds = ClampSeconds(ShowTimeSeconds);
            _settings.FontSize = AppSettings.SnapFontSize(FontSize);

            // Every non-blank line becomes its own stored setting.
            _settings.TextLines = AppSettings.SplitLines(Text);

            try
            {
                _settings.Save();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    "Failed to save settings: " + ex.Message,
                    "Save error",
                    System.Windows.MessageBoxButton.OK);
            }

            try
            {
                StartupRegistration.SetEnabled(StartWithWindows);
            }
            catch (Exception ex)
            {
                // Registry writes can be denied by policy; the preference is still stored.
                System.Windows.MessageBox.Show(
                    "Failed to set startup setting: " + ex.Message,
                    "Save error",
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
