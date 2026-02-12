using System;
using System.Windows;
using System.Windows.Input;
using FastPin.ViewModels;

namespace FastPin
{
    /// <summary>
    /// Interaction logic for ImageViewerWindow.xaml
    /// </summary>
    public partial class ImageViewerWindow : Window
    {
        private Point _dragStartPoint;
        private double _dragStartX = 0;
        private double _dragStartY = 0;
        private bool _isDragging = false;
        private double _currentZoom = 1.0;
        private const double ZoomMin = 0.1;
        private const double ZoomMax = 10.0;
        private const double ZoomStep = 0.1;

        public ImageViewerWindow(PinnedItemViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Only allow dragging the window if clicking on the window background (not the image scroll area)
            if (e.Source == this)
            {
                try
                {
                    DragMove();
                }
                catch (InvalidOperationException)
                {
                    // DragMove can throw if the mouse button is released during the drag
                    // Silently ignore this exception as it's expected behavior
                }
            }
        }

        private void ImageContainer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoomFactor = e.Delta > 0 ? 1.1 : 1 / 1.1;
            double newScaleX = ImageScaleTransform.ScaleX * zoomFactor;
            double newScaleY = ImageScaleTransform.ScaleY * zoomFactor;
            ImageScaleTransform.ScaleX = newScaleX;
            ImageScaleTransform.ScaleY = newScaleY;

            e.Handled = true;
        }

        private void ImageContainer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Start dragging to pan
            _dragStartPoint = e.GetPosition(ImageContainer);
            _dragStartX = ImageTranslateTransform.X;
            _dragStartY = ImageTranslateTransform.Y;
            _isDragging = true;
            ImageContainer.CaptureMouse();
            ImageContainer.Cursor = Cursors.Hand;
        }

        private void ImageContainer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                // Calculate translation offset based on mouse movement
                var currentPoint = e.GetPosition(ImageContainer);
                var offset = currentPoint - _dragStartPoint;

                ImageTranslateTransform.X = _dragStartX + offset.X;
                ImageTranslateTransform.Y = _dragStartY + offset.Y;
            }
        }

        private void ImageContainer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Stop dragging
            _isDragging = false;
            ImageContainer.ReleaseMouseCapture();
            ImageContainer.Cursor = Cursors.Arrow;
        }
    }
}
