using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

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

        private INotifyPropertyChanged _viewModel;

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

            // Each new line can change the size, so re-centre to keep the text in the
            // middle of the screen. This is not visible in practice: the ViewModel swaps
            // the line at the start of the appear phase, when the text is fully
            // transparent. Moving the window does not change its size, so this cannot loop.
            SizeChanged += (sender, e) =>
            {
                CenterOnWorkArea();
                EnforceAspectRatio();
            };

            DataContextChanged += OnDataContextChanged;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // Centred here rather than via WindowStartupLocation, because the actual size
            // is only known once the window has been laid out.
            CenterOnWorkArea();
            EnforceAspectRatio();
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
        /// Clears the width cap when a new line arrives, so a long message does not leave
        /// the overlay narrow for the shorter ones that follow. Runs while the text is
        /// fully transparent, so it is never visible.
        /// </summary>
        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DisplayText")
            {
                MessageText.MaxWidth = double.PositiveInfinity;
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
