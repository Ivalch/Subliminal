using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Subliminal.Converters
{
    /// <summary>Converts a hex colour string to a brush, for live swatch previews.</summary>
    [ValueConversion(typeof(string), typeof(System.Windows.Media.Brush))]
    public sealed class HexToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // HexColor speaks System.Drawing.Color; WPF brushes want System.Windows.Media.Color.
            System.Drawing.Color parsed;
            if (!HexColor.TryParse(value as string, out parsed))
            {
                // While the user is mid-typing an invalid value, show a neutral placeholder
                // rather than throwing and leaving the binding dead.
                return Frozen(Color.FromArgb(64, 156, 163, 175));
            }

            return Frozen(Color.FromArgb(parsed.A, parsed.R, parsed.G, parsed.B));
        }

        private static Brush Frozen(Color color)
        {
            // Frozen so the brush can be shared safely across the visual tree.
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
