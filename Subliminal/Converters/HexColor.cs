using System;
using System.Drawing;
using System.Globalization;

namespace Subliminal.Converters
{
    /// <summary>
    /// Parses the colour strings stored in settings. Shared so the converter and the
    /// colour-picker button agree on exactly which formats are accepted.
    /// </summary>
    internal static class HexColor
    {
        /// <summary>Accepts #RGB, #ARGB, #RRGGBB and #AARRGGBB, with or without the leading '#'.</summary>
        public static bool TryParse(string hex, out Color color)
        {
            color = Color.Empty;

            if (string.IsNullOrWhiteSpace(hex))
            {
                return false;
            }

            var text = hex.Trim();
            if (text.StartsWith("#", StringComparison.Ordinal))
            {
                text = text.Substring(1);
            }

            // Expand the #RGB shorthand into #RRGGBB.
            if (text.Length == 3)
            {
                text = new string(new[] { text[0], text[0], text[1], text[1], text[2], text[2] });
            }

            if (text.Length != 6 && text.Length != 8)
            {
                return false;
            }

            // #AARRGGBB carries its own alpha; otherwise the colour is fully opaque.
            var alpha = (byte)255;
            var offset = 0;
            if (text.Length == 8)
            {
                if (!TryParseByte(text, 0, out alpha))
                {
                    return false;
                }

                offset = 2;
            }

            byte red, green, blue;
            if (!TryParseByte(text, offset, out red) ||
                !TryParseByte(text, offset + 2, out green) ||
                !TryParseByte(text, offset + 4, out blue))
            {
                // Any non-hex character invalidates the whole value, rather than being
                // silently coerced to 0 (which would turn a typo into opaque black).
                return false;
            }

            color = Color.FromArgb(alpha, red, green, blue);
            return true;
        }

        /// <summary>Formats as #RRGGBB, which is what gets persisted.</summary>
        public static string ToHex(Color color)
        {
            return string.Format(
                "#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
        }

        private static bool TryParseByte(string text, int offset, out byte value)
        {
            return byte.TryParse(
                text.Substring(offset, 2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out value);
        }
    }
}
