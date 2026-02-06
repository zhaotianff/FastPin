using System.Globalization;
using System.Resources;
using System.ComponentModel;

namespace FastPin.Resources
{
    /// <summary>
    /// Localization service for managing application strings
    /// </summary>
    public static class LocalizationService
    {
        private static ResourceManager? _resourceManager;
        private static CultureInfo? _currentCulture;
        
        public static event PropertyChangedEventHandler? PropertyChanged;

        static LocalizationService()
        {
            _resourceManager = new ResourceManager("FastPin.Resources.Strings", typeof(LocalizationService).Assembly);
            _currentCulture = CultureInfo.CurrentUICulture;
        }

        public static string GetString(string key)
        {
            return _resourceManager?.GetString(key, _currentCulture) ?? key;
        }

        public static void SetCulture(string cultureName)
        {
            try
            {
                _currentCulture = new CultureInfo(cultureName);
                CultureInfo.CurrentUICulture = _currentCulture;
                CultureInfo.CurrentCulture = _currentCulture;
                
                // Notify that culture has changed
                OnPropertyChanged(nameof(GetString));
            }
            catch (CultureNotFoundException)
            {
                // Fall back to English if the culture is not found
                _currentCulture = new CultureInfo("en-US");
                CultureInfo.CurrentUICulture = _currentCulture;
                CultureInfo.CurrentCulture = _currentCulture;
                
                OnPropertyChanged(nameof(GetString));
            }
        }

        public static CultureInfo GetCurrentCulture()
        {
            return _currentCulture ?? CultureInfo.CurrentUICulture;
        }
        
        private static void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}
