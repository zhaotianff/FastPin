using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace FastPin.Services
{
    /// <summary>
    /// Service to register and handle global hotkeys
    /// </summary>
    public class HotkeyService : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 9000;
        
        private HwndSource? _hwndSource;
        private bool _isRegistered;

        public event EventHandler? HotkeyPressed;

        /// <summary>
        /// Register a global hotkey (Ctrl+Shift+P by default)
        /// </summary>
        public void RegisterHotkey()
        {
            if (_isRegistered)
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
                
                // Register Ctrl+Shift+P hotkey
                uint modifiers = (uint)(ModifierKeys.Control | ModifierKeys.Shift);
                uint vk = (uint)KeyInterop.VirtualKeyFromKey(Key.P);
                
                _isRegistered = RegisterHotKey(handle, HOTKEY_ID, modifiers, vk);
            }
        }

        public void UnregisterHotkey()
        {
            if (!_isRegistered)
                return;

            var window = Application.Current.MainWindow;
            if (window != null)
            {
                var windowHelper = new WindowInteropHelper(window);
                var handle = windowHelper.Handle;
                UnregisterHotKey(handle, HOTKEY_ID);
            }

            if (_hwndSource != null)
            {
                _hwndSource.RemoveHook(WndProc);
                _hwndSource = null;
            }

            _isRegistered = false;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                OnHotkeyPressed();
                handled = true;
            }

            return IntPtr.Zero;
        }

        private void OnHotkeyPressed()
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            UnregisterHotkey();
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
