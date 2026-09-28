using System.Windows;
using Subliminal.ViewModels;

namespace Subliminal
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                viewModel.SaveCommand.Execute(null);
            }

            Close();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            // Discards uncommitted edits; the persisted values are untouched.
            Close();
        }
    }
}
