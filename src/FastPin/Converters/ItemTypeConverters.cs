using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FastPin.Models;

namespace FastPin.Converters
{
    /// <summary>
    /// Converts ItemType to Visibility for conditional rendering
    /// </summary>
    public class ItemTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ItemType itemType && parameter is string targetTypeStr)
            {
                if (Enum.TryParse<ItemType>(targetTypeStr, out var target))
                {
                    return itemType == target ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
