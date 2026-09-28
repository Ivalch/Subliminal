using System;
using System.Globalization;
using System.Windows.Data;

namespace Subliminal.Converters
{
    /// <summary>
    /// Maps a 0-100 percentage onto the 0.0-1.0 range that Opacity expects.
    /// Transparancy = 100% - Opacity
    /// </summary>
    [ValueConversion(typeof(int), typeof(double))]
    public sealed class PercentToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var percent = value is int ? (int)value : 0;
            return (100 - Math.Max(0.0, Math.Min(100.0, percent))) / 100.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
