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
    public static bool isAutoRun = false;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Load and apply saved settings
        var settings = AppSettings.Load();
        FastPin.Resources.LocalizationService.SetCulture(settings.Language);

        if(e.Args != null && e.Args.Length > 0 && e.Args[0] == "-startup")
        {
            isAutoRun = true;
        }
    }
}

