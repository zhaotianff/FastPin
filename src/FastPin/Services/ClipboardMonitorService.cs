using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace FastPin.Services
{
    /// <summary>
    /// Service to monitor clipboard changes
    /// </summary>
    public class ClipboardMonitorService
    {
        private const int WM_CLIPBOARDUPDATE = 0x031D;
        private HwndSource? _hwndSource;
        private bool _isMonitoring;

        public event EventHandler? ClipboardChanged;

        public void Start()
        {
            if (_isMonitoring)
                return;

            var window = Application.Current.MainWindow;
            if (window == null)
                return;

            var windowHelper = new WindowInteropHelper(window);
            var handle = windowHelper.Handle;

            _hwndSource = HwndSource.FromHwnd(handle);
            if (_hwndSource != null)
            {
                _hwndSource.AddHook(WndProc);
                AddClipboardFormatListener(handle);
                _isMonitoring = true;
            }
        }

        public void Stop()
        {
            if (!_isMonitoring)
                return;

            var window = Application.Current.MainWindow;
            if (window != null)
            {
                var windowHelper = new WindowInteropHelper(window);
                var handle = windowHelper.Handle;
                RemoveClipboardFormatListener(handle);
            }

            if (_hwndSource != null)
            {
                _hwndSource.RemoveHook(WndProc);
                _hwndSource = null;
            }

            _isMonitoring = false;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_CLIPBOARDUPDATE)
            {
                OnClipboardChanged();
            }

            return IntPtr.Zero;
        }

        private void OnClipboardChanged()
        {
            ClipboardChanged?.Invoke(this, EventArgs.Empty);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
    }
}
