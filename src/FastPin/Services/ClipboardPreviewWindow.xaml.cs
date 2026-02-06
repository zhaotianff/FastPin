using System;
using System.Windows;
using System.Windows.Threading;
using FastPin.ViewModels;

namespace FastPin.Services
{
    /// <summary>
    /// Interaction logic for ClipboardPreviewWindow.xaml
    /// </summary>
    public partial class ClipboardPreviewWindow : Window
    {
        private MainViewModel _viewModel;
        private DispatcherTimer? _closeTimer;

        public ClipboardPreviewWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
        }

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DiscardButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Close window after a short delay when mouse leaves
            // Stop and dispose existing timer to prevent resource accumulation
            _closeTimer?.Stop();
            _closeTimer = null;
            
            _closeTimer = new DispatcherTimer();
            _closeTimer.Interval = TimeSpan.FromSeconds(0.5);
            _closeTimer.Tick += (s, args) =>
            {
                _closeTimer?.Stop();
                _closeTimer = null;
                if (!IsMouseOver)
                {
                    Close();
                }
            };
            _closeTimer.Start();
        }
    }
}
