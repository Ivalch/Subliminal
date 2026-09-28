using System.Windows;
using Subliminal.Converters;
using Subliminal.ViewModels;

namespace Subliminal
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void OnPickColorClick(object sender, RoutedEventArgs e)
        {
            if (!(DataContext is SettingsViewModel viewModel))
            {
                return;
            }

            // WPF has no built-in colour picker, so the standard dialog is used.
            // Fully qualified to keep it clear this is WinForms, not WPF.
            using (var dialog = new System.Windows.Forms.ColorDialog())
            {
                dialog.FullOpen = true;
                dialog.AnyColor = true;
                dialog.Color = viewModel.CurrentColor;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    viewModel.EditColorHex = HexColor.ToHex(dialog.Color);
                }
            }
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

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
