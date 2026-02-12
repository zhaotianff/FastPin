using System;
using System.Windows;
using FastPin.ViewModels;
using FastPin.Services;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace FastPin;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel _viewModel;
    private NotifyIconService _notifyIconService;
    
    // Pre-parsed geometry for maximize and restore icons
    private static readonly Geometry MaximizeGeometry = Geometry.Parse("M 0,0 L 10,0 L 10,10 L 0,10 Z");
    private static readonly Geometry RestoreGeometry = Geometry.Parse("M 0,2 L 8,2 L 8,10 L 0,10 Z M 2,0 L 10,0 L 10,8 L 8,8");

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
        
        // Set initial geometry for maximize/restore button
        MaximizeRestorePath.Data = MaximizeGeometry;
        
        // Subscribe to hotkey event
        _viewModel.HotkeyPressed += ViewModel_HotkeyPressed;
        
        // Subscribe to popup events
        _viewModel.OnAddTagPopupRequested += (s, e) => AddTagPopup.IsOpen = true;
        _viewModel.OnAddTagPopupClosed += (s, e) => AddTagPopup.IsOpen = false;
        
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

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var configWindow = new ConfigWindow();
        if (configWindow.ShowDialog() == true)
        {
            // Settings were saved, apply language change
            var settings = Models.AppSettings.Load();
            _viewModel.CurrentLanguage = settings.Language;
        }
    }

    private void DrawerMenuButton_Click(object sender, RoutedEventArgs e)
    {
        // Toggle drawer visibility with animation
        if (DrawerPanel.Visibility == Visibility.Visible)
        {
            // Slide out animation
            var slideOut = new DoubleAnimation
            {
                From = 0,
                To = -DrawerPanel.ActualWidth,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            
            slideOut.Completed += (s, args) =>
            {
                DrawerPanel.Visibility = Visibility.Collapsed;
                DrawerPanel.RenderTransform = new System.Windows.Media.TranslateTransform(0, 0);
            };
            
            var transform = new System.Windows.Media.TranslateTransform();
            DrawerPanel.RenderTransform = transform;
            transform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideOut);
        }
        else
        {
            DrawerPanel.Visibility = Visibility.Visible;
            
            // Slide in animation
            var slideIn = new DoubleAnimation
            {
                From = -DrawerPanel.ActualWidth,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            
            var transform = new System.Windows.Media.TranslateTransform();
            DrawerPanel.RenderTransform = transform;
            transform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideIn);
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
            MaximizeRestorePath.Data = MaximizeGeometry;
        }
        else
        {
            WindowState = WindowState.Maximized;
            MaximizeRestorePath.Data = RestoreGeometry;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void CancelAddTag_Click(object sender, RoutedEventArgs e)
    {
        AddTagPopup.IsOpen = false;
        _viewModel.NewTagName = string.Empty;
    }

    private void AddTagButton_Click(object sender, RoutedEventArgs e)
    {
        AddTagPopup.IsOpen = false;
        _viewModel.NewTagName = string.Empty;
    }

    private void RemoveTagButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button)
        {
            var item = button.Tag as PinnedItemViewModel;
            var tagName = button.CommandParameter as string;
            
            if (item != null && !string.IsNullOrWhiteSpace(tagName))
            {
                // Set the selected item so RemoveTag can work
                _viewModel.SelectedItem = item;
                _viewModel.RemoveTagCommand.Execute(tagName);
            }
        }
    }
}
