using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
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
        
        public string? LastClipboardSource { get; private set; }

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
            // Try to detect clipboard source window
            LastClipboardSource = GetClipboardSourceApplication();
            ClipboardChanged?.Invoke(this, EventArgs.Empty);
        }
        
        private string? GetClipboardSourceApplication()
        {
            try
            {
                IntPtr clipboardOwner = GetClipboardOwner();
                if (clipboardOwner == IntPtr.Zero)
                    return null;

                // Get process ID from window handle
                GetWindowThreadProcessId(clipboardOwner, out uint processId);
                if (processId == 0)
                    return null;

                // Get process name
                using (var process = Process.GetProcessById((int)processId))
                {
                    // Try to get the main window title first
                    if (!string.IsNullOrWhiteSpace(process.MainWindowTitle))
                    {
                        return process.MainWindowTitle;
                    }
                    
                    // Otherwise use process name
                    return process.ProcessName;
                }
            }
            catch
            {
                return null;
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetClipboardOwner();
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    }
}
