using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Subliminal.Mvvm;
using Subliminal.Services;

namespace Subliminal.ViewModels
{
    /// <summary>
    /// Drives the always-on-top text overlay with a repeating cycle:
    /// appear (invisible -> chosen transparency), hold, then fade back out,
    /// at which point a new random line starts the cycle again.
    /// </summary>
    public class OverlayViewModel : ObservableObject
    {
        private const double TickMilliseconds = 50;

        private enum Phase { Appear, Show, Fade }

        private readonly AppSettings _settings;
        private readonly DispatcherTimer _timer;
        private readonly Random _random = new Random();

        private List<string> _lines = new List<string>();

        private string _displayText = string.Empty;
        private int _transparency = 250;
        private string _colorHex = "#FFFFFF";
        private int _appearSeconds = 5;
        private int _showTimeSeconds = 10;
        private int _fontSize = 52;
        private double _currentOpacity;

        private Phase _phase = Phase.Appear;
        private DateTime _phaseStarted = DateTime.UtcNow;

        public OverlayViewModel(AppSettings settings)
        {
            _settings = settings;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(TickMilliseconds) };
            _timer.Tick += OnTick;
        }

        /// <summary>Text currently shown in the overlay.</summary>
        public string DisplayText
        {
            get { return _displayText; }
            set { SetProperty(ref _displayText, value); }
        }

        /// <summary>Target transparency from settings. 255 is fully invisible.</summary>
        public int Transparency
        {
            get { return _transparency; }
            set
            {
                if (SetProperty(ref _transparency, value))
                {
                    OnPropertyChanged("DesiredOpacity");
                }
            }
        }

        /// <summary>Overlay colour as #RRGGBB, taken from settings.</summary>
        public string ColorHex
        {
            get { return _colorHex; }
            set { SetProperty(ref _colorHex, value); }
        }

        /// <summary>Seconds for each of the appear and fade ramps, 1-10.</summary>
        public int AppearSeconds
        {
            get { return _appearSeconds; }
            set
            {
                if (SetProperty(ref _appearSeconds, value))
                {
                    OnPropertyChanged("AppearDuration");
                }
            }
        }

        /// <summary>Seconds a line stays at its chosen transparency, 1-10.</summary>
        public int ShowTimeSeconds
        {
            get { return _showTimeSeconds; }
            set
            {
                if (SetProperty(ref _showTimeSeconds, value))
                {
                    OnPropertyChanged("ShowDuration");
                }
            }
        }

        /// <summary>Overlay text size in points, taken from settings.</summary>
        public int FontSize
        {
            get { return _fontSize; }
            set { SetProperty(ref _fontSize, value); }
        }

        /// <summary>
        /// Animated 0.0-1.0 opacity the view binds to. 0 means invisible, which is where
        /// each cycle starts. This is driven by the phase timer, not the transparency
        /// setting directly, so the appear/fade ramp can be drawn.
        /// </summary>
        public double CurrentOpacity
        {
            get { return _currentOpacity; }
            set { SetProperty(ref _currentOpacity, value); }
        }

        public double AppearDuration { get { return ClampSeconds(AppearSeconds); } }

        public double ShowDuration { get { return ClampSeconds(ShowTimeSeconds); } }

        /// <summary>Opacity the line settles at once it has fully appeared.</summary>
        public double DesiredOpacity
        {
            get { return Math.Max(0.0, Math.Min(255.0, 255 - Transparency)) / 255.0; }
        }

        /// <summary>False when no texts are saved, so the overlay renders nothing.</summary>
        public bool HasText
        {
            get { return _lines.Count > 0; }
        }

        public void Start()
        {
            Refresh();
            PickRandomLine();
            BeginPhase(Phase.Appear);
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        /// <summary>Re-reads colour, transparency, timings and texts from the settings.</summary>
        public void Refresh()
        {
            _lines = _settings.TextLines ?? new List<string>();
            ColorHex = _settings.EditColorHex;
            Transparency = _settings.Transparency;
            AppearSeconds = _settings.AppearSeconds;
            ShowTimeSeconds = _settings.ShowTimeSeconds;
            FontSize = _settings.FontSize;

            OnPropertyChanged(nameof(HasText));

            if (_lines.Count == 0)
            {
                DisplayText = string.Empty;
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            var elapsed = (DateTime.UtcNow - _phaseStarted).TotalSeconds;

            switch (_phase)
            {
                case Phase.Appear:
                    // Ramp from invisible up to the chosen transparency.
                    CurrentOpacity = DesiredOpacity * Eased(Progress(elapsed, AppearDuration));
                    if (elapsed >= AppearDuration)
                    {
                        BeginPhase(Phase.Show);
                    }

                    break;

                case Phase.Show:
                    CurrentOpacity = DesiredOpacity;
                    if (elapsed >= ShowDuration)
                    {
                        BeginPhase(Phase.Fade);
                    }

                    break;

                case Phase.Fade:
                    // Symmetric with the appear ramp, so the same setting drives both.
                    CurrentOpacity = DesiredOpacity * (1 - Eased(Progress(elapsed, AppearDuration)));
                    if (elapsed >= AppearDuration)
                    {
                        // The old line is fully invisible by now, so swapping in a new one
                        // is invisible itself.
                        PickRandomLine();
                        BeginPhase(Phase.Appear);
                    }

                    break;


            }
        }

        private void BeginPhase(Phase phase)
        {
            _phase = phase;
            _phaseStarted = DateTime.UtcNow;

            if (phase == Phase.Appear)
            {
                // Every cycle starts fully transparent.
                CurrentOpacity = 0;
            }
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

        private static double Progress(double elapsed, double duration)
        {
            if (duration <= 0)
            {
                return 1;
            }

            return Math.Max(0.0, Math.Min(1.0, elapsed / duration));
        }

        /// <summary>Smoothstep, so the line eases in and out instead of ramping linearly.</summary>
        private static double Eased(double t)
        {
            return t * t * (3 - (2 * t));
        }

        private static double ClampSeconds(int value)
        {
            if (value < 1)
            {
                return 1;
            }

            return value > 10 ? 10 : value;
        }
    }
}
