using System.ComponentModel;
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
            // Cast rather than 'is T x' pattern matching: that is C# 7 and this project
            // targets C# 6 so it opens in Visual Studio 2015.
            var viewModel = DataContext as SettingsViewModel;
            if (viewModel == null)
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

        /// <summary>
        /// There are no Save/Cancel buttons, so closing the dialog is what commits the
        /// settings. Saving here rather than on Closed means the values are already
        /// written by the time the window is torn down.
        ///
        /// The text fields bind with UpdateSourceTrigger=PropertyChanged for this reason:
        /// with LostFocus, the field the user is currently typing in would not have been
        /// pushed to the ViewModel yet, so the last edit would be silently lost on close.
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            var viewModel = DataContext as SettingsViewModel;
            if (viewModel != null)
            {
                viewModel.SaveCommand.Execute(null);
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
