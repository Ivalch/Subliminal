using System;
using System.Windows;
using Subliminal.ViewModels;

namespace Subliminal
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // The window is created here (instead of via StartupUri) so the ViewModel
            // can be supplied as the DataContext. MainWindow.xaml.cs stays logic-free.
            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };

            MainWindow = window;
            window.Show();
        }
    }
}
