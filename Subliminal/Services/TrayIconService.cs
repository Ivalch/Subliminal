using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;

namespace Subliminal.Services
{
    /// <summary>
    /// Owns the system tray icon. A field on the application (via this service) must
    /// hold the <see cref="TaskbarIcon"/> for its whole lifetime; if it is garbage
    /// collected, the icon silently disappears from the tray.
    /// </summary>
    public sealed class TrayIconService : IDisposable
    {
        private readonly TaskbarIcon _taskbarIcon;
        private bool _disposed;

        public event EventHandler SettingsRequested;
        public event EventHandler ExitRequested;

        public TrayIconService()
        {
            _taskbarIcon = new TaskbarIcon
            {
                // IconSource is typed ImageSource, not string, so a URI string cannot be
                // assigned directly in C# (the WPF type converter only applies in XAML).
                IconSource = LoadIcon(),
                ToolTipText = "Subliminal",
                ContextMenu = BuildContextMenu()
            };

            _taskbarIcon.TrayMouseDoubleClick += OnTrayMouseDoubleClick;
        }

        private static ImageSource LoadIcon()
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri("pack://application:,,,/Subliminal.ico", UriKind.Absolute);
            image.EndInit();
            image.Freeze();
            return image;
        }

        private ContextMenu BuildContextMenu()
        {
            var menu = new ContextMenu();

            var settings = new MenuItem { Header = "_Settings..." };
            settings.Click += (s, e) => SettingsRequested?.Invoke(this, EventArgs.Empty);
            menu.Items.Add(settings);

            menu.Items.Add(new Separator());

            var exit = new MenuItem { Header = "E_xit" };
            exit.Click += (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty);
            menu.Items.Add(exit);

            return menu;
        }

        private void OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            // Settings is the only window this app has, so double-click opens it.
            SettingsRequested?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _taskbarIcon.TrayMouseDoubleClick -= OnTrayMouseDoubleClick;
            _taskbarIcon.Dispose();
        }
    }
}
