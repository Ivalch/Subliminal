using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Subliminal.Mvvm
{
    /// <summary>
    /// Minimal INotifyPropertyChanged base for the ViewModels.
    ///
    /// Hand-written rather than generated: source generators need a modern Roslyn, which
    /// Visual Studio 2015 (C# 6) does not have. Keeping this local means the project
    /// builds with the oldest supported toolchain, and needs no package reference.
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Assigns <paramref name="value"/> to <paramref name="field"/> and raises a change
        /// notification when it differs. Returns true when the value actually changed.
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
