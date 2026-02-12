using System;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using FastPin.ViewModels;

namespace FastPin.Services
{
    /// <summary>
    /// Service to manage system tray notify icon
    /// </summary>
    public class NotifyIconService : IDisposable
    {
        private TaskbarIcon? _notifyIcon;
        private MainViewModel? _viewModel;
        private ClipboardPreviewWindow? _previewWindow;

        public void Initialize(MainViewModel viewModel)
        {
            _viewModel = viewModel;
            
            _notifyIcon = new TaskbarIcon
            {
                ToolTipText = "FastPin - Clipboard Manager"
            };

            // Try to load icon, but don't fail if it doesn't exist
            try
            {
                _notifyIcon.IconSource = new System.Windows.Media.Imaging.BitmapImage(
                    new Uri("pack://application:,,,/pin.ico", UriKind.Absolute));
            }
            catch
            {
                // Icon not found, use default
            }

            _notifyIcon.TrayMouseMove += NotifyIcon_TrayMouseMove;
            _notifyIcon.TrayLeftMouseUp += NotifyIcon_TrayLeftMouseUp;
            
            // Create context menu
            var contextMenu = new System.Windows.Controls.ContextMenu();
            
            var openMenuItem = new System.Windows.Controls.MenuItem { Header = "Open FastPin" };
            openMenuItem.Click += (s, e) => OpenMainWindow();
            contextMenu.Items.Add(openMenuItem);
            
            contextMenu.Items.Add(new System.Windows.Controls.Separator());
            
            var exitMenuItem = new System.Windows.Controls.MenuItem { Header = "Exit" };
            exitMenuItem.Click += (s, e) => ExitApplication();
            contextMenu.Items.Add(exitMenuItem);
            
            _notifyIcon.ContextMenu = contextMenu;
        }

        private void NotifyIcon_TrayMouseMove(object sender, RoutedEventArgs e)
        {
            if (_viewModel?.ClipboardPreviewType == null)
                return;

            ShowPreviewWindow();
        }

        private void NotifyIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            ShowPreviewWindow();
        }

        private void ShowPreviewWindow()
        {
            if (_previewWindow == null && _viewModel != null)
            {
                _previewWindow = new ClipboardPreviewWindow(_viewModel);
                _previewWindow.Closed += (s, e) => _previewWindow = null;
                
                // Position near the system tray
                _previewWindow.Left = SystemParameters.PrimaryScreenWidth - _previewWindow.Width - 10;
                _previewWindow.Top = SystemParameters.PrimaryScreenHeight - _previewWindow.Height - 50;
                
                _previewWindow.Show();
            }
            else if (_previewWindow != null)
            {
                _previewWindow.Activate();
            }
        }

        private void OpenMainWindow()
        {
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Show();
                Application.Current.MainWindow.WindowState = WindowState.Normal;
                Application.Current.MainWindow.Activate();
            }
        }

        private void ExitApplication()
        {
            Application.Current.Shutdown();
        }

        public void Dispose()
        {
            _notifyIcon?.Dispose();
            _previewWindow?.Close();
        }
    }
}
