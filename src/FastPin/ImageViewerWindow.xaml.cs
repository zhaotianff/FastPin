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
        private Point _scrollMousePoint;
        private double _hOffset = 0;
        private double _vOffset = 0;
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

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Get mouse position relative to the ScrollViewer viewport
            var mousePosition = e.GetPosition(ImageScrollViewer);
            
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

            ImageScaleTransform.ScaleX = _currentZoom;
            ImageScaleTransform.ScaleY = _currentZoom;

            // Adjust scroll position to zoom toward cursor
            if (ImageScrollViewer.ScrollableWidth > 0 || ImageScrollViewer.ScrollableHeight > 0)
            {
                double zoomRatio = _currentZoom / oldZoom;
                double offsetX = (ImageScrollViewer.HorizontalOffset + mousePosition.X) * zoomRatio - mousePosition.X;
                double offsetY = (ImageScrollViewer.VerticalOffset + mousePosition.Y) * zoomRatio - mousePosition.Y;
                
                ImageScrollViewer.ScrollToHorizontalOffset(offsetX);
                ImageScrollViewer.ScrollToVerticalOffset(offsetY);
            }

            e.Handled = true;
        }

        private void ScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Start dragging to scroll
            _scrollMousePoint = e.GetPosition(ImageScrollViewer);
            _hOffset = ImageScrollViewer.HorizontalOffset;
            _vOffset = ImageScrollViewer.VerticalOffset;
            _isDragging = true;
            ImageScrollViewer.CaptureMouse();
            ImageScrollViewer.Cursor = Cursors.Hand;
        }

        private void ScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                // Calculate scroll offset based on mouse movement
                var currentPoint = e.GetPosition(ImageScrollViewer);
                var offset = currentPoint - _scrollMousePoint;

                ImageScrollViewer.ScrollToHorizontalOffset(_hOffset - offset.X);
                ImageScrollViewer.ScrollToVerticalOffset(_vOffset - offset.Y);
            }
        }

        private void ScrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Stop dragging
            _isDragging = false;
            ImageScrollViewer.ReleaseMouseCapture();
            ImageScrollViewer.Cursor = Cursors.Arrow;
        }
    }
}
