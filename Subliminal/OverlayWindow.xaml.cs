using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Subliminal
{
    /// <summary>
    /// Always-on-top, fully transparent text overlay shown in the centre of the screen.
    /// </summary>
    public partial class OverlayWindow : Window
    {
        private const int GwlExStyle = -20;
        private const int WsExTransparent = 0x00000020;   // clicks fall through to the window below
        private const int WsExToolWindow = 0x00000080;    // keep out of the Alt+Tab list
        private const int WsExNoActivate = 0x08000000;    // do not steal focus from the active app

        /// <summary>The window may never be wider than this many times its height.</summary>
        private const double MaxAspectRatio = 5.0;

        private readonly Random _placement = new Random();

        private INotifyPropertyChanged _viewModel;
        private bool _placementPending;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public OverlayWindow()
        {
            InitializeComponent();

            // The window sizes itself to the text (SizeToContent) so a message that wraps
            // to several lines is never cropped. These caps keep a very long message from
            // growing the window past the usable screen area.
            var area = SystemParameters.WorkArea;
            MaxWidth = area.Width * 0.8;
            MaxHeight = area.Height * 0.7;

            // Each new line can change the size, so the 5:1 cap has to be re-checked. Only
            // the aspect ratio is enforced here; the position is chosen per message, since
            // moving the window does not change its size and so cannot loop.
            SizeChanged += (sender, e) => EnforceAspectRatio();

            DataContextChanged += OnDataContextChanged;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // Catches the case where the placement was requested before the window had
            // been laid out and so had nothing to position.
            TryPlacePending();
        }

        /// <summary>
        /// Caps the text width so the window is never wider than 5x its height. Narrowing
        /// the text makes it wrap onto more lines, which makes the window taller, so this
        /// only ever converges downwards and cannot oscillate between two widths.
        /// </summary>
        private void EnforceAspectRatio()
        {
            if (ActualHeight <= 0 || MessageText == null)
            {
                return;
            }

            var allowed = ActualHeight * MaxAspectRatio;
            if (ActualWidth > allowed)
            {
                MessageText.MaxWidth = allowed;
            }
        }

        /// <summary>
        /// Moves the overlay to a random spot on each new message, so consecutive lines do
        /// not sit in the same place. The window is kept whole inside the work area: the
        /// random offset is limited to the slack left after the window is placed, and a
        /// window larger than the work area is pinned to its top-left corner.
        /// </summary>
        private void PlaceRandomlyInWorkArea()
        {
            var area = SystemParameters.WorkArea;

            var slackX = area.Width - ActualWidth;
            var slackY = area.Height - ActualHeight;

            if (slackX <= 0 || slackY <= 0)
            {
                Left = area.Left;
                Top = area.Top;
                return;
            }

            Left = area.Left + (int)(_placement.NextDouble() * slackX);
            Top = area.Top + (int)(_placement.NextDouble() * slackY);
        }

        private void RequestPlacement()
        {
            _placementPending = true;

            // At ContextIdle the layout for the new text has settled, so the window has a
            // real size to position. Runs while the text is fully transparent, so the
            // move is not visible.
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(TryPlacePending));
        }

        private void TryPlacePending()
        {
            if (!_placementPending || ActualWidth <= 0 || ActualHeight <= 0)
            {
                return;
            }

            EnforceAspectRatio();
            PlaceRandomlyInWorkArea();
            _placementPending = false;
        }

        /// <summary>
        /// A new line needs a fresh width cap and a fresh position. Runs while the text is
        /// fully transparent, so neither is ever visible.
        /// </summary>
        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DisplayText")
            {
                MessageText.MaxWidth = double.PositiveInfinity;
                RequestPlacement();
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }

            _viewModel = e.NewValue as INotifyPropertyChanged;

            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        private void CenterOnWorkArea()
        {
            var area = SystemParameters.WorkArea;
            Left = area.Left + (area.Width - ActualWidth) / 2;
            Top = area.Top + (area.Height - ActualHeight) / 2;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // The handle only exists once the source is initialised, so this cannot
            // be done in the constructor.
            var handle = new WindowInteropHelper(this).Handle;
            if (handle == IntPtr.Zero)
            {
                return;
            }

            var style = GetWindowLong(handle, GwlExStyle);
            SetWindowLong(handle, GwlExStyle, style | WsExTransparent | WsExToolWindow | WsExNoActivate);
        }
    }
}
