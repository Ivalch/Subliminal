using System;
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

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public OverlayWindow()
        {
            InitializeComponent();

            // A fixed size is deliberate. With SizeToContent the window would resize and
            // re-centre every time the displayed line changed, which makes the overlay
            // visibly jump around.
            var area = SystemParameters.WorkArea;
            Width = area.Width * 0.7;
            Height = area.Height * 0.25;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // Centred here rather than via WindowStartupLocation, because the actual size
            // is only known once the window has been laid out.
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
