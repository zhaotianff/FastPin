using System.Windows;
using FastPin.ViewModels;

namespace FastPin.Services
{
    /// <summary>
    /// Interaction logic for ClipboardPreviewWindow.xaml
    /// </summary>
    public partial class ClipboardPreviewWindow : Window
    {
        private MainViewModel _viewModel;

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
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                if (!IsMouseOver)
                {
                    Close();
                }
            };
            timer.Start();
        }
    }
}
