using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FastPin.Converters
{
    /// <summary>
    /// Converts a hex color string to a Color object
    /// </summary>
    public class HexToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Colors.DodgerBlue; // Default color

            var hexString = value.ToString();
            if (string.IsNullOrWhiteSpace(hexString))
                return Colors.DodgerBlue;

            try
            {
                // Remove # if present
                hexString = hexString.TrimStart('#');
                
                // Handle different hex formats
                if (hexString.Length == 6)
                {
                    // RGB format
                    var r = System.Convert.ToByte(hexString.Substring(0, 2), 16);
                    var g = System.Convert.ToByte(hexString.Substring(2, 2), 16);
                    var b = System.Convert.ToByte(hexString.Substring(4, 2), 16);
                    return Color.FromRgb(r, g, b);
                }
                else if (hexString.Length == 8)
                {
                    // ARGB format
                    var a = System.Convert.ToByte(hexString.Substring(0, 2), 16);
                    var r = System.Convert.ToByte(hexString.Substring(2, 2), 16);
                    var g = System.Convert.ToByte(hexString.Substring(4, 2), 16);
                    var b = System.Convert.ToByte(hexString.Substring(6, 2), 16);
                    return Color.FromArgb(a, r, g, b);
                }
                
                return Colors.DodgerBlue;
            }
            catch
            {
                return Colors.DodgerBlue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }
            return "#0078D4";
        }
    }

    /// <summary>
    /// Converts null to Collapsed visibility
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null || (value is string str && string.IsNullOrWhiteSpace(str))
                ? System.Windows.Visibility.Collapsed
                : System.Windows.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
