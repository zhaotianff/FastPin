using System.Configuration;
using System.Data;
using System.Windows;
using FastPin.Models;

namespace FastPin;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Load and apply saved settings
        var settings = AppSettings.Load();
        FastPin.Resources.LocalizationService.SetCulture(settings.Language);
    }
}

