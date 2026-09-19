using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace LearningReminder.Services
{
    /// <summary>
    /// 全局快捷键：Ctrl+Alt+L 打开主界面、Ctrl+Alt+K 立即检查全部。
    /// 通过消息窗口接收 WM_HOTKEY；注册失败时静默忽略（通常是被其他软件占用）。
    /// </summary>
    public sealed class HotkeyService : IDisposable
    {
        private const int WmHotkey = 0x0312;
        private const int HotkeyOpen = 1;
        private const int HotkeyCheckAll = 2;
        private const uint ModAlt = 0x0001;
        private const uint ModControl = 0x0002;
        private const uint KeyL = 0x4C;
        private const uint KeyK = 0x4B;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly HwndSource _source;
        private bool _disposed;

        /// <summary>创建消息窗口并注册快捷键。</summary>
        public HotkeyService()
        {
            HwndSourceParameters parameters = new HwndSourceParameters("LearningReminder.Hotkeys")
            {
                // HWND_MESSAGE：只接收消息、不显示的窗口
                ParentWindow = new IntPtr(-3),
                WindowStyle = 0
            };
            _source = new HwndSource(parameters);
            _source.AddHook(WndProc);

            if (!RegisterHotKey(_source.Handle, HotkeyOpen, ModControl | ModAlt, KeyL))
            {
                FileLogger.Info("全局快捷键 Ctrl+Alt+L 注册失败（可能已被其他软件占用）");
            }

            if (!RegisterHotKey(_source.Handle, HotkeyCheckAll, ModControl | ModAlt, KeyK))
            {
                FileLogger.Info("全局快捷键 Ctrl+Alt+K 注册失败（可能已被其他软件占用）");
            }
        }

        /// <summary>请求打开主界面</summary>
        public event EventHandler? OpenRequested;

        /// <summary>请求立即检查全部任务</summary>
        public event EventHandler? CheckAllRequested;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != WmHotkey)
            {
                return IntPtr.Zero;
            }

            int id = wParam.ToInt32();
            if (id == HotkeyOpen)
            {
                OpenRequested?.Invoke(this, EventArgs.Empty);
                handled = true;
            }
            else if (id == HotkeyCheckAll)
            {
                CheckAllRequested?.Invoke(this, EventArgs.Empty);
                handled = true;
            }

            return IntPtr.Zero;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            UnregisterHotKey(_source.Handle, HotkeyOpen);
            UnregisterHotKey(_source.Handle, HotkeyCheckAll);
            _source.Dispose();
        }
    }
}