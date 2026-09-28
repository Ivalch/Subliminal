using System;
using System.Collections.Generic;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Subliminal.Services;

namespace Subliminal.ViewModels
{
    /// <summary>
    /// Drives the always-on-top text overlay: picks a random line from the saved texts
    /// and swaps it every <see cref="ChangeInterval"/>.
    /// </summary>
    public partial class OverlayViewModel : ObservableObject
    {
        public static readonly TimeSpan ChangeInterval = TimeSpan.FromSeconds(10);

        private readonly AppSettings _settings;
        private readonly DispatcherTimer _timer;
        private readonly Random _random = new Random();
        private List<string> _lines = new List<string>();

        public OverlayViewModel(AppSettings settings)
        {
            _settings = settings;

            _timer = new DispatcherTimer { Interval = ChangeInterval };
            _timer.Tick += OnTick;
        }

        /// <summary>Text currently shown in the overlay.</summary>
        [ObservableProperty]
        private string _displayText = string.Empty;

        /// <summary>Overlay colour as #RRGGBB, taken from settings.</summary>
        [ObservableProperty]
        private string _colorHex = "#FFFFFF";

        /// <summary>
        /// Overlay transparency percentage, taken from settings. 95 means 95% transparent;
        /// PercentToOpacityConverter turns that into the 0.05 opacity the view uses.
        /// </summary>
        [ObservableProperty]
        private int _transparency = 95;

        /// <summary>False when no texts are saved, so callers can avoid an empty overlay.</summary>
        public bool HasText
        {
            get { return _lines.Count > 0; }
        }

        public void Start()
        {
            Refresh();
            PickRandomLine();
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        /// <summary>Re-reads colour, transparency and texts from the persisted settings.</summary>
        public void Refresh()
        {
            _lines = _settings.TextLines ?? new List<string>();
            ColorHex = _settings.EditColorHex;
            Transparency = _settings.Transparency;

            OnPropertyChanged(nameof(HasText));

            if (_lines.Count == 0)
            {
                DisplayText = string.Empty;
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            // Re-reading on every tick means edits saved in Settings show up here with no
            // extra wiring, at the cost of up to one interval of delay.
            Refresh();
            PickRandomLine();
        }

        private void PickRandomLine()
        {
            if (_lines.Count == 0)
            {
                DisplayText = string.Empty;
                return;
            }

            DisplayText = _lines[_random.Next(_lines.Count)];
        }
    }
}
