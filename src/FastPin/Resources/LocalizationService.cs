using System.Globalization;
using System.Resources;

namespace FastPin.Resources
{
    /// <summary>
    /// Localization service for managing application strings
    /// </summary>
    public static class LocalizationService
    {
        private static ResourceManager? _resourceManager;
        private static CultureInfo? _currentCulture;

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
            _currentCulture = new CultureInfo(cultureName);
            CultureInfo.CurrentUICulture = _currentCulture;
            CultureInfo.CurrentCulture = _currentCulture;
        }

        public static CultureInfo GetCurrentCulture()
        {
            return _currentCulture ?? CultureInfo.CurrentUICulture;
        }
    }
}
