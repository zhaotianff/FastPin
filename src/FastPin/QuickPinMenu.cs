using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FastPin
{
    /// <summary>
    /// Popup menu that appears at mouse position for quick actions
    /// </summary>
    public partial class QuickPinMenu : Window
    {
        public event EventHandler<string>? ActionSelected;
        private bool isManualClose = false;

        public QuickPinMenu()
        {
            InitializeComponent();
            
            // Position window at mouse cursor
            var mousePosition = GetMousePosition();
            this.Left = mousePosition.X - 100; // Center the menu at cursor
            this.Top = mousePosition.Y - 100;
            
            // Window settings
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            this.Background = Brushes.Transparent;
            this.Topmost = true;
            this.ShowInTaskbar = false;
            this.ResizeMode = ResizeMode.NoResize;
            this.Width = 200;
            this.Height = 200;
            this.isManualClose = false;

            // Close when losing focus
            this.Deactivated += (s, e) =>
            {
                if (this.isManualClose == false)
                {
                    this.Close();
                }
            };
            
            // Close when Esc key is pressed
            this.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Escape)
                {
                    this.isManualClose = true;
                    this.Close();
                }
            };
        }

        private void InitializeComponent()
        {
            var canvas = new Canvas
            {
                Width = 200,
                Height = 200
            };

            // Create ellipse background
            var ellipse = new Ellipse
            {
                Width = 180,
                Height = 180,
                Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                Stroke = new SolidColorBrush(Color.FromRgb(225, 223, 221)),
                StrokeThickness = 2,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 15,
                    ShadowDepth = 3,
                    Opacity = 0.4
                }
            };
            Canvas.SetLeft(ellipse, 10);
            Canvas.SetTop(ellipse, 10);
            canvas.Children.Add(ellipse);

            // Create three button sections
            // Top section - Pin Text
            var textButton = CreateEllipseButton("📝", "Pin Text", "PinText", 100, 30);
            canvas.Children.Add(textButton);

            // Bottom Left section - Pin Image
            var imageButton = CreateEllipseButton("🖼️", "Pin Image", "PinImage", 50, 140);
            canvas.Children.Add(imageButton);

            // Bottom Right section - Pin File
            var fileButton = CreateEllipseButton("📁", "Pin File", "PinFile", 150, 140);
            canvas.Children.Add(fileButton);

            // Add divider lines to create three sections
            var line1 = new Line
            {
                X1 = 100,
                Y1 = 100,
                X2 = 50,
                Y2 = 140,
                Stroke = new SolidColorBrush(Color.FromRgb(225, 223, 221)),
                StrokeThickness = 1,
                Opacity = 0.5
            };
            canvas.Children.Add(line1);

            var line2 = new Line
            {
                X1 = 100,
                Y1 = 100,
                X2 = 150,
                Y2 = 140,
                Stroke = new SolidColorBrush(Color.FromRgb(225, 223, 221)),
                StrokeThickness = 1,
                Opacity = 0.5
            };
            canvas.Children.Add(line2);

            this.Content = canvas;
        }

        private Border CreateEllipseButton(string icon, string text, string action, double left, double top)
        {
            var button = new Border
            {
                Width = 70,
                Height = 60,
                Background = Brushes.Transparent,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            var stackPanel = new StackPanel
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };

            var iconText = new TextBlock
            {
                Text = icon,
                FontSize = 24,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5)
            };
            stackPanel.Children.Add(iconText);

            var labelText = new TextBlock
            {
                Text = text,
                FontSize = 11,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            stackPanel.Children.Add(labelText);

            button.Child = stackPanel;

            // Position button
            Canvas.SetLeft(button, left - 35);
            Canvas.SetTop(button, top - 30);

            // Click handler
            button.MouseLeftButtonDown += (s, e) =>
            {
                ActionSelected?.Invoke(this, action);
                this.isManualClose = true;
                this.Close();
            };

            // Hover effect
            button.MouseEnter += (s, e) =>
            {
                button.Background = new SolidColorBrush(Color.FromArgb(30, 0, 120, 212));
            };

            button.MouseLeave += (s, e) =>
            {
                button.Background = Brushes.Transparent;
            };

            return button;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        private static System.Windows.Point GetMousePosition()
        {
            GetCursorPos(out POINT point);
            return new System.Windows.Point(point.X, point.Y);
        }
    }
}
