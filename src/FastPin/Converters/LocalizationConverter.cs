using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using FastPin.Resources;

namespace FastPin.Converters
{
    /// <summary>
    /// Converter for accessing localized strings in XAML
    /// </summary>
    public class LocalizeExtension : MarkupExtension
    {
        public string Key { get; set; } = string.Empty;

        public LocalizeExtension()
        {
        }

        public LocalizeExtension(string key)
        {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return LocalizationService.GetString(Key);
        }
    }

    /// <summary>
    /// Converter for dynamic localization in bindings
    /// </summary>
    public class LocalizationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string key)
            {
                return LocalizationService.GetString(key);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
