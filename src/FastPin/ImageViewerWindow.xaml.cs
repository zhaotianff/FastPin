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
            // Get mouse position relative to the ImageContainer
            var mousePosition = e.GetPosition(ImageContainer);
            
            // Calculate the old zoom level
            double oldZoom = _currentZoom;
            
            // Zoom image with mouse wheel
            if (e.Delta > 0)
            {
                _currentZoom = Math.Min(_currentZoom + ZoomStep, ZoomMax);
            }
            else
            {
                _currentZoom = Math.Max(_currentZoom - ZoomStep, ZoomMin);
            }

            // Calculate zoom ratio
            double zoomRatio = _currentZoom / oldZoom;
            
            // Adjust the translate transform to zoom toward the mouse cursor
            // We need to adjust translation so the point under cursor stays in the same position
            // Formula: newTranslate = mousePos - (mousePos - oldTranslate) * zoomRatio
            ImageTranslateTransform.X = mousePosition.X - (mousePosition.X - ImageTranslateTransform.X) * zoomRatio;
            ImageTranslateTransform.Y = mousePosition.Y - (mousePosition.Y - ImageTranslateTransform.Y) * zoomRatio;
            
            // Apply new scale
            ImageScaleTransform.ScaleX = _currentZoom;
            ImageScaleTransform.ScaleY = _currentZoom;

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
