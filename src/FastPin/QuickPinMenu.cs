using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FastPin
{
    /// <summary>
    /// Popup menu that appears at mouse position for quick actions
    /// </summary>
    public partial class QuickPinMenu : Window
    {
        public event EventHandler<string>? ActionSelected;

        public QuickPinMenu()
        {
            InitializeComponent();
            
            // Position window at mouse cursor
            var mousePosition = GetMousePosition();
            this.Left = mousePosition.X;
            this.Top = mousePosition.Y;
            
            // Window settings
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            this.Background = Brushes.Transparent;
            this.Topmost = true;
            this.ShowInTaskbar = false;
            this.ResizeMode = ResizeMode.NoResize;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            
            // Deactivate when losing focus
            this.Deactivated += (s, e) => this.Close();
        }

        private void InitializeComponent()
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(225, 223, 221)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 10,
                    ShadowDepth = 2,
                    Opacity = 0.3
                },
                Padding = new Thickness(5)
            };

            var stackPanel = new StackPanel();
            
            // Pin Text button
            var pinTextButton = CreateMenuButton("📝 Pin Text", "PinText");
            stackPanel.Children.Add(pinTextButton);
            
            // Pin Image button
            var pinImageButton = CreateMenuButton("🖼️ Pin Image", "PinImage");
            stackPanel.Children.Add(pinImageButton);
            
            // Pin File button
            var pinFileButton = CreateMenuButton("📁 Pin File", "PinFile");
            stackPanel.Children.Add(pinFileButton);
            
            border.Child = stackPanel;
            this.Content = border;
        }

        private Button CreateMenuButton(string text, string action)
        {
            var button = new Button
            {
                Content = text,
                Padding = new Thickness(15, 8, 15, 8),
                Margin = new Thickness(2),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Cursor = System.Windows.Input.Cursors.Hand,
                FontSize = 14,
                Width = 180
            };

            button.Click += (s, e) =>
            {
                ActionSelected?.Invoke(this, action);
                this.Close();
            };

            // Hover effect
            button.MouseEnter += (s, e) =>
            {
                button.Background = new SolidColorBrush(Color.FromRgb(0, 120, 212));
                button.Foreground = Brushes.White;
            };

            button.MouseLeave += (s, e) =>
            {
                button.Background = Brushes.Transparent;
                button.Foreground = Brushes.Black;
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
