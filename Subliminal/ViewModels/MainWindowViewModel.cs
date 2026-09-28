using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Subliminal.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _title = "Subliminal";

        [ObservableProperty]
        private string _status = "Ready";

        [RelayCommand]
        private void Start()
        {
            Status = "Running";
        }
    }
}
