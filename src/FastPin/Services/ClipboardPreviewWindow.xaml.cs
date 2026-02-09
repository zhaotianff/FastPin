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
            
            // Adjust window size based on clipboard content
            Loaded += ClipboardPreviewWindow_Loaded;
        }

        private void ClipboardPreviewWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustWindowSize();
        }

        private void AdjustWindowSize()
        {
            // Only adjust for images
            if (_viewModel.ClipboardPreviewImageSource != null)
            {
                var image = _viewModel.ClipboardPreviewImageSource;
                var imageWidth = image.PixelWidth;
                var imageHeight = image.PixelHeight;

                if (imageWidth > 0 && imageHeight > 0)
                {
                    // Calculate aspect ratio
                    double aspectRatio = (double)imageWidth / imageHeight;

                    // Set base dimensions for preview
                    const double maxPreviewWidth = 500;
                    const double maxPreviewHeight = 400;
                    const double minPreviewWidth = 250;
                    const double minPreviewHeight = 200;

                    double previewWidth;
                    double previewHeight;

                    // Calculate preview dimensions while maintaining aspect ratio
                    if (aspectRatio > 1) // Landscape
                    {
                        previewWidth = Math.Min(maxPreviewWidth, Math.Max(minPreviewWidth, imageWidth * 0.5));
                        previewHeight = previewWidth / aspectRatio;

                        if (previewHeight > maxPreviewHeight)
                        {
                            previewHeight = maxPreviewHeight;
                            previewWidth = previewHeight * aspectRatio;
                        }
                    }
                    else // Portrait or square
                    {
                        previewHeight = Math.Min(maxPreviewHeight, Math.Max(minPreviewHeight, imageHeight * 0.5));
                        previewWidth = previewHeight * aspectRatio;

                        if (previewWidth > maxPreviewWidth)
                        {
                            previewWidth = maxPreviewWidth;
                            previewHeight = previewWidth / aspectRatio;
                        }
                    }

                    // Add padding for borders and buttons (approximately 100px total)
                    Width = Math.Max(minPreviewWidth, previewWidth + 60);
                    Height = Math.Max(minPreviewHeight, previewHeight + 120);
                }
            }
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
