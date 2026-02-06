using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using FastPin.Resources;

namespace FastPin.Converters
{
    /// <summary>
    /// Converter for accessing localized strings in XAML with dynamic updates
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
            var binding = new Binding("Value")
            {
                Source = new LocalizedString(Key),
                Mode = BindingMode.OneWay
            };
            
            return binding.ProvideValue(serviceProvider);
        }
    }

    /// <summary>
    /// Helper class to provide dynamic localized strings
    /// </summary>
    public class LocalizedString : INotifyPropertyChanged
    {
        private readonly string _key;

        public LocalizedString(string key)
        {
            _key = key;
            LocalizationService.PropertyChanged += (s, e) => OnPropertyChanged(nameof(Value));
        }

        public string Value => LocalizationService.GetString(_key);

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
