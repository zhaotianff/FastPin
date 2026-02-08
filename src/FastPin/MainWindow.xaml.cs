using System.Windows;
using FastPin.ViewModels;
using FastPin.Services;

namespace FastPin;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel _viewModel;
    private NotifyIconService _notifyIconService;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
        
        // Subscribe to hotkey event
        _viewModel.HotkeyPressed += ViewModel_HotkeyPressed;
        
        // Initialize notify icon
        _notifyIconService = new NotifyIconService();
        _notifyIconService.Initialize(_viewModel);
        
        Loaded += MainWindow_Loaded;
        Closed += MainWindow_Closed;
    }

    private void ViewModel_HotkeyPressed(object? sender, EventArgs e)
    {
        // Show the quick pin menu at mouse cursor
        var menu = new QuickPinMenu();
        menu.ActionSelected += (s, action) =>
        {
            switch (action)
            {
                case "PinText":
                    _viewModel.PinTextCommand.Execute(null);
                    break;
                case "PinImage":
                    _viewModel.PinImageCommand.Execute(null);
                    break;
                case "PinFile":
                    _viewModel.PinFileCommand.Execute(null);
                    break;
            }
        };
        menu.Show();
        menu.Activate();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel.StartClipboardMonitoring();
        _viewModel.StartHotkeyMonitoring();
    }

    private void MainWindow_Closed(object? sender, System.EventArgs e)
    {
        _viewModel.StopClipboardMonitoring();
        _viewModel.StopHotkeyMonitoring();
        _notifyIconService.Dispose();
    }

    private async void CacheCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.CheckBox checkBox)
        {
            var itemViewModel = checkBox.DataContext as PinnedItemViewModel;
            if (itemViewModel != null)
            {
                await _viewModel.ToggleFileCacheAsync(itemViewModel);
            }
        }
    }
}
